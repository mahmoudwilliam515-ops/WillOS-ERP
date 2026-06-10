FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/EnterpriseERP.Api/EnterpriseERP.Api.csproj", "src/EnterpriseERP.Api/"]
COPY ["src/EnterpriseERP.Application/EnterpriseERP.Application.csproj", "src/EnterpriseERP.Application/"]
COPY ["src/EnterpriseERP.Domain/EnterpriseERP.Domain.csproj", "src/EnterpriseERP.Domain/"]
COPY ["src/EnterpriseERP.SharedKernel/EnterpriseERP.SharedKernel.csproj", "src/EnterpriseERP.SharedKernel/"]
COPY ["src/EnterpriseERP.Shared/EnterpriseERP.Shared.csproj", "src/EnterpriseERP.Shared/"]
COPY ["src/EnterpriseERP.Infrastructure/EnterpriseERP.Infrastructure.csproj", "src/EnterpriseERP.Infrastructure/"]

RUN dotnet restore "src/EnterpriseERP.Api/EnterpriseERP.Api.csproj"

COPY . .
WORKDIR "/src/src/EnterpriseERP.Api"
RUN dotnet build "EnterpriseERP.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnterpriseERP.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseERP.Api.dll"]
