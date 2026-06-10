using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.EInvoice;
using EnterpriseERP.SharedKernel.Common;
using MediatR;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.EInvoice.Commands;

public class GenerateZatcaInvoiceCommand : IRequest<Guid>
{
    public Guid SalesInvoiceId { get; set; }
}

public class GenerateZatcaInvoiceCommandHandler : IRequestHandler<GenerateZatcaInvoiceCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public GenerateZatcaInvoiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(GenerateZatcaInvoiceCommand request, CancellationToken cancellationToken)
    {
        // 1. In a real scenario, we'd fetch the SalesInvoice and calculate hash based on UBL XML
        // Mock generation for ZATCA Phase 2
        var previousHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ=="; // Mock PIH
        
        var invoiceDoc = new EInvoiceDocument(
            request.SalesInvoiceId, 
            "SalesInvoice", 
            EInvoiceProvider.ZATCA,
            previousHash);

        // Mock hashing and Crypto stamp
        string rawData = $"{invoiceDoc.Uuid}|{request.SalesInvoiceId}|{DateTime.UtcNow:O}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        string invoiceHash = Convert.ToBase64String(hashBytes);
        
        string cryptoStamp = "MEYCIQDaM8w3/yG4Q9rT1L+Y... (Mock ECDSA Signature)";
        
        // ZATCA TLV QR Code implementation
        string qrCode = GenerateTlvQrCode("Sample Company", "300123456700003", DateTime.UtcNow, 1150.00m, 150.00m);

        invoiceDoc.SetZatcaData(invoiceHash, cryptoStamp, qrCode);
        
        // As per Section 8: clearance API -> ZATCA_CLEARED
        invoiceDoc.UpdateStatus(EInvoiceStatus.Cleared, "200", "Reported successfully to ZATCA");

        await _unitOfWork.Repository<EInvoiceDocument>().AddAsync(invoiceDoc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoiceDoc.Id;
    }

    private string GenerateTlvQrCode(string sellerName, string vatNumber, DateTime timestamp, decimal total, decimal vatAmount)
    {
        var tags = new List<byte[]>
        {
            EncodeTlv(1, sellerName),
            EncodeTlv(2, vatNumber),
            EncodeTlv(3, timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ")),
            EncodeTlv(4, total.ToString("F2")),
            EncodeTlv(5, vatAmount.ToString("F2"))
        };

        var qrBytes = tags.SelectMany(x => x).ToArray();
        return Convert.ToBase64String(qrBytes);
    }

    private byte[] EncodeTlv(byte tag, string value)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var tlv = new byte[2 + valueBytes.Length];
        tlv[0] = tag;
        tlv[1] = (byte)valueBytes.Length;
        Array.Copy(valueBytes, 0, tlv, 2, valueBytes.Length);
        return tlv;
    }
}
