namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface ISecretsService
{
    /// <summary>
    /// Gets a secret value by key. RULE-SEC02: الأسرار تُقرأ من Environment Variables فقط.
    /// </summary>
    string GetSecret(string key);
    
    /// <summary>
    /// Gets a secret value by key with default value if not found.
    /// </summary>
    string GetSecret(string key, string defaultValue);
    
    /// <summary>
    /// Validates that all required secrets are configured.
    /// </summary>
    bool ValidateRequiredSecrets(string[] requiredKeys);
}
