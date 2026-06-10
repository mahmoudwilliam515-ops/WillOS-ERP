using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/v1/lineage")]
public class DataLineageController : ControllerBase
{
    /// <summary>
    /// Fetch column lineage (Data Lineage Phase 6)
    /// </summary>
    [HttpGet("column")]
    public async Task<IActionResult> GetColumnLineage([FromQuery] string table, [FromQuery] string column)
    {
        return Ok(new 
        { 
            Success = true, 
            Table = table,
            Column = column,
            Source = "ERP.OLTP.JournalEntryLines",
            Transformations = new[] { "CurrencyConversion(DailyRate)", "AccountAggregation(SUM)" },
            Confidence = "HIGH"
        });
    }
}
