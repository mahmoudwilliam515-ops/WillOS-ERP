using EnterpriseERP.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace EnterpriseERP.Infrastructure.Services;

public class SecretsService : ISecretsService
{
    private readonly IConfiguration _configuration;

    public SecretsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetSecret(string key)
    {
        var value = Environment.GetEnvironmentVariable(key) ?? _configuration[key];
        if (string.IsNullOrEmpty(value))
            throw new InvalidOperationException($"Secret '{key}' is missing.");
        return value;
    }

    public string GetSecret(string key, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(key) ?? _configuration[key];
        return !string.IsNullOrEmpty(value) ? value : defaultValue;
    }

    public bool ValidateRequiredSecrets(string[] requiredKeys)
    {
        return requiredKeys.All(key => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key) ?? _configuration[key]));
    }
}
