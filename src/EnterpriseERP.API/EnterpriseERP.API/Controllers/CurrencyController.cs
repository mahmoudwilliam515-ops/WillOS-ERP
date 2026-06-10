using EnterpriseERP.Application.Features.Accounting.Commands.ExchangeRateRevaluation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController : ControllerBase
{
    private readonly IMediator _mediator;

    public CurrencyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// تشغيل إعادة تقييم أسعار الصرف لنهاية الفترة وفق IAS 21.
    /// ينشئ قيود فروق العملة الأجنبية تلقائياً.
    /// </summary>
    [HttpPost("revaluation")]
    public async Task<IActionResult> RunRevaluation([FromBody] RunExchangeRateRevaluationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
