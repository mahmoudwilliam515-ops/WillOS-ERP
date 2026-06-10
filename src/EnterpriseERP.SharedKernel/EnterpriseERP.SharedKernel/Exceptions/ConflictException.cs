namespace EnterpriseERP.SharedKernel.Exceptions;

/// <summary>
/// Thrown when a state conflict prevents the operation (HTTP 409).
/// Used for: duplicate invoice_number, invalid state transitions, SOD violations.
/// Carries a machine-readable <see cref="ErrorCode"/> per Phase 9 §9.3.3.
/// </summary>
public class ConflictException : Exception
{
    public string ErrorCode { get; }

    public ConflictException(string message, string errorCode = "ERR_CONFLICT")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
