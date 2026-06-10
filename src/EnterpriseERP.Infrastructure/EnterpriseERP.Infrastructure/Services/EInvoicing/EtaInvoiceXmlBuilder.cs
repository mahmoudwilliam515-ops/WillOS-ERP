using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace EnterpriseERP.Infrastructure.Services.EInvoicing;

/// <summary>Simplified ETA-compatible UBL-style document for Egyptian e-invoicing (v1 sandbox).</summary>
public static class EtaInvoiceXmlBuilder
{
    public static string Build(
        string issuerTaxId,
        string issuerName,
        string receiverTaxId,
        string receiverName,
        string internalId,
        DateTime issueDate,
        decimal subTotal,
        decimal taxAmount,
        decimal totalAmount,
        IEnumerable<(string ItemName, decimal Quantity, decimal UnitPrice, decimal LineTotal)> lines)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("ETAInvoiceDocument",
                new XAttribute("version", "1.0"),
                new XElement("Issuer",
                    new XElement("TaxRegistrationNumber", issuerTaxId),
                    new XElement("Name", issuerName)),
                new XElement("Receiver",
                    new XElement("TaxRegistrationNumber", receiverTaxId),
                    new XElement("Name", receiverName)),
                new XElement("Invoice",
                    new XElement("InternalID", internalId),
                    new XElement("IssueDate", issueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    new XElement("DocumentCurrencyCode", "EGP"),
                    new XElement("TaxExclusiveAmount", subTotal.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement("TaxAmount", taxAmount.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement("TaxInclusiveAmount", totalAmount.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement("InvoiceLines",
                        lines.Select((line, index) =>
                            new XElement("Line",
                                new XElement("ID", (index + 1).ToString(CultureInfo.InvariantCulture)),
                                new XElement("ItemName", line.ItemName),
                                new XElement("Quantity", line.Quantity.ToString("F3", CultureInfo.InvariantCulture)),
                                new XElement("UnitPrice", line.UnitPrice.ToString("F2", CultureInfo.InvariantCulture)),
                                new XElement("LineTotal", line.LineTotal.ToString("F2", CultureInfo.InvariantCulture))))))));

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        {
            doc.Save(writer);
        }

        return sb.ToString();
    }
}
