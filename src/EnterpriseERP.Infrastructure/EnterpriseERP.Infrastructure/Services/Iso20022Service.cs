using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Treasury;

namespace EnterpriseERP.Infrastructure.Services;

public class Iso20022Service : IIso20022Service
{
    public Task<string> GeneratePain001XmlAsync(IEnumerable<PaymentVoucher> vouchers)
    {
        XNamespace ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";
        
        var firstVoucher = vouchers.First();
        var debtorAccount = firstVoucher.BankAccount;

        var document = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(ns + "Document",
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XElement(ns + "CstmrCdtTrfInitn",
                    // Group Header
                    new XElement(ns + "GrpHdr",
                        new XElement(ns + "MsgId", $"MSG-{DateTime.UtcNow:yyyyMMddHHmmss}"),
                        new XElement(ns + "CreDtTm", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")),
                        new XElement(ns + "NbOfTxs", vouchers.Count()),
                        new XElement(ns + "CtrlSum", vouchers.Sum(v => v.Amount).ToString("F2")),
                        new XElement(ns + "InitgPty",
                            new XElement(ns + "Nm", "Enterprise ERP Corp")
                        )
                    ),
                    // Payment Information
                    new XElement(ns + "PmtInf",
                        new XElement(ns + "PmtInfId", $"PMT-{DateTime.UtcNow:yyyyMMdd}"),
                        new XElement(ns + "PmtMtd", "TRF"),
                        new XElement(ns + "NbOfTxs", vouchers.Count()),
                        new XElement(ns + "CtrlSum", vouchers.Sum(v => v.Amount).ToString("F2")),
                        new XElement(ns + "PmtTpInf",
                            new XElement(ns + "SvcLvl",
                                new XElement(ns + "Cd", "SEPA")
                            )
                        ),
                        new XElement(ns + "ReqdExctnDt", DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd")),
                        new XElement(ns + "Dbtr",
                            new XElement(ns + "Nm", "Enterprise ERP Corp")
                        ),
                        new XElement(ns + "DbtrAcct",
                            new XElement(ns + "Id",
                                new XElement(ns + "IBAN", debtorAccount?.IBAN ?? "UNKNOWN")
                            ),
                            new XElement(ns + "Ccy", debtorAccount?.Currency ?? "EGP")
                        ),
                        new XElement(ns + "DbtrAgt",
                            new XElement(ns + "FinInstnId",
                                new XElement(ns + "BIC", "DEBTORBICXX") // Placeholder
                            )
                        ),
                        // Credit Transfer Transactions
                        vouchers.Select(v => new XElement(ns + "CdtTrfTxInf",
                            new XElement(ns + "PmtId",
                                new XElement(ns + "EndToEndId", v.VoucherNumber)
                            ),
                            new XElement(ns + "Amt",
                                new XElement(ns + "InstdAmt", new XAttribute("Ccy", debtorAccount?.Currency ?? "EGP"), v.Amount.ToString("F2"))
                            ),
                            new XElement(ns + "CdtrAgt",
                                new XElement(ns + "FinInstnId",
                                    new XElement(ns + "BIC", "CREDITORBICXX")
                                )
                            ),
                            new XElement(ns + "Cdtr",
                                new XElement(ns + "Nm", v.SupplierId.HasValue ? v.SupplierId.Value.ToString() : "Unknown Supplier")
                            ),
                            new XElement(ns + "CdtrAcct",
                                new XElement(ns + "Id",
                                    new XElement(ns + "IBAN", "UNKNOWN")
                                )
                            ),
                            new XElement(ns + "RmtInf",
                                new XElement(ns + "Ustrd", $"Payment for {v.VoucherNumber}")
                            )
                        ))
                    )
                )
            )
        );

        return Task.FromResult(document.ToString());
    }
}

