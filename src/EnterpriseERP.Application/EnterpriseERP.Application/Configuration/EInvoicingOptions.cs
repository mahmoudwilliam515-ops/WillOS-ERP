namespace EnterpriseERP.Application.Configuration;

public class EInvoicingOptions
{
    public const string SectionName = "EInvoicing";

    /// <summary>Sandbox simulates gateway acceptance without external HTTP.</summary>
    public string Mode { get; set; } = "Sandbox";

    public EInvoicingGatewayOptions Zatca { get; set; } = new();
    public EInvoicingGatewayOptions Eta { get; set; } = new();
}

public class EInvoicingGatewayOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
