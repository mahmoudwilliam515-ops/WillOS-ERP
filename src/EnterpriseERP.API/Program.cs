using System.Text;
using EnterpriseERP.API.Middleware;
using EnterpriseERP.Application;
using EnterpriseERP.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Application", "EnterpriseERP")
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/enterpriseerp-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("Starting EnterpriseERP API");

    // Use Serilog
    builder.Host.UseSerilog();

var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

// Add layers dependency injections
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Add Redis Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "EnterpriseERP_";
});

if (!useInMemoryDatabase)
{
    // Add Hangfire
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddHangfireServer();
}

builder.Services.AddScoped<EnterpriseERP.Application.Common.Interfaces.Services.ICurrentUserService, EnterpriseERP.API.Services.CurrentUserService>();
builder.Services.AddScoped<EnterpriseERP.Application.Common.Interfaces.Services.ISecretsService, EnterpriseERP.Infrastructure.Services.SecretsService>();
builder.Services.AddScoped<EnterpriseERP.Application.Common.Interfaces.Services.IAccountMappingService, EnterpriseERP.Application.Services.AccountMappingService>();
builder.Services.AddScoped<EnterpriseERP.Application.Common.Interfaces.Services.IPeriodClosingService, EnterpriseERP.Application.Services.PeriodClosingService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddHostedService<EnterpriseERP.API.BackgroundServices.LowStockMonitorService>();
if (!useInMemoryDatabase)
{
    builder.Services.AddHostedService<EnterpriseERP.API.BackgroundServices.DatabaseBackupService>();
}
builder.Services.AddHostedService<EnterpriseERP.Application.BackgroundServices.BillingCycleJob>();
builder.Services.AddHostedService<EnterpriseERP.Application.BackgroundServices.ManufacturingAlertWorker>();

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100, // 100 requests
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1) // per 1 minute
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Configure Health Checks
var healthChecks = builder.Services.AddHealthChecks();

if (!useInMemoryDatabase)
{
    healthChecks.AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
        name: "SQL Server",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "sqlserver" }
    );
}

healthChecks
    .AddCheck("Self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck("Memory", () =>
    {
        var memory = GC.GetTotalMemory(false);
        var memoryMB = memory / 1024 / 1024;
        return memoryMB > 1000
            ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded($"Memory usage: {memoryMB}MB")
            : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"Memory usage: {memoryMB}MB");
    });

// Add Health Checks UI
builder.Services.AddHealthChecksUI(settings =>
{
    settings.SetEvaluationTimeInSeconds(30);
    settings.MaximumHistoryEntriesPerEndpoint(50);
    settings.AddHealthCheckEndpoint("EnterpriseERP API", "/health");
}).AddInMemoryStorage();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin => true) // Allow any origin in development
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:Key"]
    ?? throw new InvalidOperationException("JwtSettings:Key is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

var app = builder.Build();

// Skip seeding if running EF migrations
if (!args.Contains("--ef"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<EnterpriseERP.Infrastructure.Data.DatabaseSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else 
{
    app.UseHttpsRedirection();
}

app.UseCors("DefaultCors");

app.UseRateLimiter(); // Use Rate Limiting before Authentication

app.UseAuthentication();
app.UseAuthorization();

if (!useInMemoryDatabase)
{
    // Add Hangfire Dashboard
    app.UseHangfireDashboard();
}

// ──────────────────────────────────────────────────────────────────────
// Global Exception Handler — DomainException→400, Validation→422,
// NotFound→404, System→500 (all via GlobalExceptionHandler)
// ──────────────────────────────────────────────────────────────────────
app.UseExceptionHandler();

app.MapControllers();

app.MapHub<EnterpriseERP.API.Hubs.InventoryHub>("/hubs/inventory");

// Advanced Health check endpoint
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

// Health Checks UI
app.UseHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-ui-api";
});

app.Run();

Log.Information("EnterpriseERP API stopped successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "EnterpriseERP API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }


