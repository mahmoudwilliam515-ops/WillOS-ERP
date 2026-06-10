using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Configuration;
using EnterpriseERP.Application.Features.SalesInvoices.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.Infrastructure.Services.EInvoicing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EnterpriseERP.Infrastructure.Services;

public class EInvoicingService : IEInvoicingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EInvoicingOptions _options;

    public EInvoicingService(
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        IOptions<EInvoicingOptions> options)
    {
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<EInvoicePreviewDto> GetPreviewAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var ctx = await LoadContextAsync(invoiceId, cancellationToken);
        var xml = BuildXml(ctx);
        var hash = ComputeSha256Hex(xml);
        var qr = ctx.Authority == EInvoiceAuthority.Zatca ? BuildZatcaQr(ctx) : null;

        var canSubmit = ctx.Invoice.Status == InvoiceStatus.Approved
                        && ctx.Invoice.EInvoiceStatus is EInvoiceSubmissionStatus.NotSubmitted
                            or EInvoiceSubmissionStatus.Rejected;

        return new EInvoicePreviewDto
        {
            SalesInvoiceId = ctx.Invoice.Id,
            InvoiceNumber = ctx.Invoice.InvoiceNumber,
            Authority = ctx.Authority.ToString(),
            SubmissionStatus = ctx.Invoice.EInvoiceStatus.ToString(),
            SubmissionUuid = ctx.Invoice.EInvoiceUuid,
            QrPayloadBase64 = qr ?? ctx.Invoice.EInvoiceQrPayload,
            XmlHash = hash,
            ResponseMessage = ctx.Invoice.EInvoiceResponseMessage,
            SubmittedAt = ctx.Invoice.EInvoiceSubmittedAt,
            XmlPreview = xml,
            CanSubmit = canSubmit && ctx.Authority != EInvoiceAuthority.None,
            BlockReason = canSubmit ? null : GetBlockReason(ctx)
        };
    }

    public async Task<string> GenerateInvoiceXmlAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var ctx = await LoadContextAsync(invoiceId, cancellationToken);
        return BuildXml(ctx);
    }

    public async Task<EInvoiceSubmissionResultDto> SubmitInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var preview = await GetPreviewAsync(invoiceId, cancellationToken);
        if (!preview.CanSubmit)
        {
            return new EInvoiceSubmissionResultDto
            {
                Success = false,
                Authority = preview.Authority,
                SubmissionStatus = preview.SubmissionStatus,
                Message = preview.BlockReason ?? "Invoice cannot be submitted."
            };
        }

        var ctx = await LoadContextAsync(invoiceId, cancellationToken);
        var xml = BuildXml(ctx);
        var hash = ComputeSha256Hex(xml);
        var qr = ctx.Authority == EInvoiceAuthority.Zatca ? BuildZatcaQr(ctx) : null;

        ctx.Invoice.EInvoiceAuthority = ctx.Authority;
        ctx.Invoice.EInvoiceStatus = EInvoiceSubmissionStatus.Pending;
        ctx.Invoice.EInvoiceXmlHash = hash;
        ctx.Invoice.EInvoiceQrPayload = qr;
        ctx.Invoice.EInvoiceSubmittedAt = DateTime.UtcNow;

        string message;
        bool accepted;

        if (IsSandbox())
        {
            ctx.Invoice.EInvoiceUuid = Guid.NewGuid().ToString();
            ctx.Invoice.EInvoiceStatus = EInvoiceSubmissionStatus.Accepted;
            message = $"Sandbox acceptance ({ctx.Authority}) — configure production gateway in appsettings.";
            accepted = true;
        }
        else
        {
            var gatewayResult = await PostToGatewayAsync(ctx, xml, cancellationToken);
            accepted = gatewayResult.Success;
            ctx.Invoice.EInvoiceUuid = gatewayResult.Uuid;
            ctx.Invoice.EInvoiceStatus = accepted ? EInvoiceSubmissionStatus.Accepted : EInvoiceSubmissionStatus.Rejected;
            message = gatewayResult.Message;
        }

        ctx.Invoice.EInvoiceResponseMessage = message;
        _unitOfWork.Repository<SalesInvoice>().Update(ctx.Invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EInvoiceSubmissionResultDto
        {
            Success = accepted,
            Authority = ctx.Authority.ToString(),
            SubmissionStatus = ctx.Invoice.EInvoiceStatus.ToString(),
            SubmissionUuid = ctx.Invoice.EInvoiceUuid,
            QrPayloadBase64 = ctx.Invoice.EInvoiceQrPayload,
            Message = message
        };
    }

    private bool IsSandbox() =>
        string.Equals(_options.Mode, "Sandbox", StringComparison.OrdinalIgnoreCase);

    private async Task<(bool Success, string? Uuid, string Message)> PostToGatewayAsync(
        InvoiceContext ctx, string xml, CancellationToken cancellationToken)
    {
        var gateway = ctx.Authority == EInvoiceAuthority.Zatca ? _options.Zatca : _options.Eta;
        if (string.IsNullOrWhiteSpace(gateway.BaseUrl))
        {
            return (false, null, "Gateway BaseUrl is not configured.");
        }

        var client = _httpClientFactory.CreateClient("EInvoicing");
        client.BaseAddress = new Uri(gateway.BaseUrl.TrimEnd('/') + "/");
        if (!string.IsNullOrWhiteSpace(gateway.ApiKey))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", gateway.ApiKey);

        var payload = JsonSerializer.Serialize(new
        {
            document = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml)),
            internalId = ctx.Invoice.InvoiceNumber,
            authority = ctx.Authority.ToString()
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("documents/submit", content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return (false, null, $"Gateway error {(int)response.StatusCode}: {body}");

        return (true, Guid.NewGuid().ToString(), "Submitted to tax authority gateway.");
    }

    private static string? GetBlockReason(InvoiceContext ctx)
    {
        if (ctx.Authority == EInvoiceAuthority.None)
            return "Company country is not configured for ZATCA (SA) or ETA (EG).";
        if (ctx.Invoice.Status != InvoiceStatus.Approved)
            return "Invoice must be approved before e-invoice submission.";
        if (ctx.Invoice.EInvoiceStatus == EInvoiceSubmissionStatus.Accepted)
            return "Invoice already accepted by tax authority.";
        if (ctx.Invoice.EInvoiceStatus == EInvoiceSubmissionStatus.Pending)
            return "Submission is pending.";
        if (string.IsNullOrWhiteSpace(ctx.Company.TaxRegistrationNumber))
            return "Company tax registration number is required.";
        return null;
    }

    private string BuildXml(InvoiceContext ctx)
    {
        if (ctx.Authority == EInvoiceAuthority.Zatca)
        {
            return EtaInvoiceXmlBuilder.Build(
                ctx.Company.TaxRegistrationNumber,
                ctx.Company.Name,
                ctx.Customer.TaxNumber,
                ctx.Customer.Name,
                ctx.Invoice.InvoiceNumber,
                ctx.Invoice.InvoiceDate,
                ctx.Invoice.SubTotal,
                ctx.Invoice.TaxAmount,
                ctx.Invoice.TotalAmount,
                ctx.Lines);
        }

        return EtaInvoiceXmlBuilder.Build(
            ctx.Company.TaxRegistrationNumber,
            ctx.Company.Name,
            ctx.Customer.TaxNumber,
            ctx.Customer.Name,
            ctx.Invoice.InvoiceNumber,
            ctx.Invoice.InvoiceDate,
            ctx.Invoice.SubTotal,
            ctx.Invoice.TaxAmount,
            ctx.Invoice.TotalAmount,
            ctx.Lines);
    }

    private static string BuildZatcaQr(InvoiceContext ctx) =>
        ZatcaQrEncoder.Encode(
            ctx.Company.Name,
            ctx.Company.TaxRegistrationNumber,
            ctx.Invoice.InvoiceDate.ToUniversalTime(),
            ctx.Invoice.TotalAmount,
            ctx.Invoice.TaxAmount);

    private static string ComputeSha256Hex(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private async Task<InvoiceContext> LoadContextAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Repository<SalesInvoice>().Query()
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

        if (invoice == null)
            throw new SalesDomainException($"Sales invoice {invoiceId} not found.");

        var company = await _unitOfWork.Repository<Company>().GetByIdAsync(invoice.CompanyId);
        if (company == null)
            throw new SalesDomainException("Company not found for invoice.");

        var lineEntities = await _unitOfWork.Repository<SalesInvoiceLine>()
            .FindAsync(l => l.SalesInvoiceId == invoiceId);

        var lines = new List<(string ItemName, decimal Quantity, decimal UnitPrice, decimal LineTotal)>();
        foreach (var line in lineEntities)
        {
            var item = await _unitOfWork.Repository<Item>().GetByIdAsync(line.ItemId);
            lines.Add((item?.Name ?? line.ItemId.ToString(), line.Quantity, line.UnitPrice, line.LineTotal));
        }

        var authority = ResolveAuthority(company.CountryCode);

        return new InvoiceContext(invoice, company, invoice.Customer, authority, lines);
    }

    private static EInvoiceAuthority ResolveAuthority(string countryCode) =>
        countryCode?.Trim().ToUpperInvariant() switch
        {
            "SA" or "SAU" or "KSA" => EInvoiceAuthority.Zatca,
            "EG" or "EGY" => EInvoiceAuthority.Eta,
            _ => EInvoiceAuthority.None
        };

    private sealed record InvoiceContext(
        SalesInvoice Invoice,
        Company Company,
        Customer Customer,
        EInvoiceAuthority Authority,
        List<(string ItemName, decimal Quantity, decimal UnitPrice, decimal LineTotal)> Lines);
}
