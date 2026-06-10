using EnterpriseERP.Application.Features.EInvoice.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EInvoiceController : ControllerBase
{
    private readonly IMediator _mediator;

    public EInvoiceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("zatca/generate")]
    public async Task<IActionResult> GenerateZatcaInvoice([FromBody] GenerateZatcaInvoiceCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("eta/submit")]
    public async Task<IActionResult> SubmitEtaInvoice([FromBody] SubmitEtaInvoiceCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("documents")]
    public async Task<IActionResult> GetEInvoiceDocuments()
    {
        var query = new EnterpriseERP.Application.Features.EInvoice.Queries.GetEInvoiceDocumentsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
