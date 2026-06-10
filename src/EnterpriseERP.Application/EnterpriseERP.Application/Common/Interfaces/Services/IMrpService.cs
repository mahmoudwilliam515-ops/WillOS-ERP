using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IMrpService
{
    /// <summary>
    /// Runs the MRP calculation for a specific company and period.
    /// </summary>
    Task<List<MrpRecommendationDto>> RunMrpAsync(Guid companyId, DateTime forecastEndDate, CancellationToken cancellationToken = default);
}

public class MrpRecommendationDto
{
    public Guid ItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal SuggestedOrderQuantity { get; set; }
    public DateTime SuggestedDate { get; set; }
    public int LeadTimeDays { get; set; }
    public RecommendationType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public enum RecommendationType
{
    PurchaseOrder = 0,
    ProductionOrder = 1,
    Transfer = 2
}
