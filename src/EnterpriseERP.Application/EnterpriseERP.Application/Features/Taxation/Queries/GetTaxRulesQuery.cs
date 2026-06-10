using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Taxation;
using EnterpriseERP.SharedKernel.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Taxation.Queries;

public record TaxRuleDto(Guid Id, Guid TaxCategoryId, string RuleName, decimal Rate, string TaxType, string Region, bool IsActive, DateTime ValidFrom, DateTime? ValidTo);

public class GetTaxRulesQuery : IRequest<List<TaxRuleDto>>
{
}

public class GetTaxRulesQueryHandler : IRequestHandler<GetTaxRulesQuery, List<TaxRuleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTaxRulesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TaxRuleDto>> Handle(GetTaxRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _unitOfWork.Repository<TaxRule>().GetAllAsync();
        
        return rules.Select(r => new TaxRuleDto(
            r.Id,
            r.TaxCategoryId,
            r.RuleName,
            r.Rate,
            r.TaxType,
            r.Region,
            r.IsActive,
            r.ValidFrom,
            r.ValidTo
        )).ToList();
    }
}
