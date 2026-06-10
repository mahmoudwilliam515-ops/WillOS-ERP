namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IPaymentGatewayService
{
    /// <summary>
    /// Initializes a payment session and returns a checkout URL or token.
    /// </summary>
    Task<string> InitializePaymentAsync(Guid invoiceId, decimal amount, string currency);

    /// <summary>
    /// Verifies the webhook signature or payment status.
    /// </summary>
    Task<bool> VerifyPaymentAsync(string paymentReference);
}
