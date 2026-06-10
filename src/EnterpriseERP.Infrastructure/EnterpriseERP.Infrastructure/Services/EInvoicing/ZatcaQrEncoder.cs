using System.Text;

namespace EnterpriseERP.Infrastructure.Services.EInvoicing;

/// <summary>ZATCA Phase 1 TLV QR (tags 1–5) encoded as Base64.</summary>
public static class ZatcaQrEncoder
{
    public static string Encode(
        string sellerName,
        string vatRegistrationNumber,
        DateTime invoiceDateTimeUtc,
        decimal invoiceTotalWithVat,
        decimal vatTotal)
    {
        var timestamp = invoiceDateTimeUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        var total = invoiceTotalWithVat.ToString("F2");
        var vat = vatTotal.ToString("F2");

        return EncodeTlvBase64(
            (1, sellerName),
            (2, vatRegistrationNumber),
            (3, timestamp),
            (4, total),
            (5, vat));
    }

    private static string EncodeTlvBase64(params (byte Tag, string Value)[] fields)
    {
        using var stream = new MemoryStream();
        foreach (var (tag, value) in fields)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            if (valueBytes.Length > 255)
                throw new InvalidOperationException($"ZATCA TLV field {tag} exceeds 255 bytes.");

            stream.WriteByte(tag);
            stream.WriteByte((byte)valueBytes.Length);
            stream.Write(valueBytes, 0, valueBytes.Length);
        }

        return Convert.ToBase64String(stream.ToArray());
    }
}
