using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Taxation;
using EnterpriseERP.SharedKernel.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Taxation.Commands;

public class CreateTaxRuleCommand : IRequest<Guid>
{
    public Guid TaxCategoryId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string TaxType { get; set; } = "VAT";
    public string Region { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
}

public class CreateTaxRuleCommandHandler : IRequestHandler<CreateTaxRuleCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaxRuleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTaxRuleCommand request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<TaxCategory>().GetByIdAsync(request.TaxCategoryId);
        if (category == null)
            throw new Exception("Tax Category not found");

        var taxRule = new TaxRule(
            request.TaxCategoryId,
            request.RuleName,
            request.Rate,
            request.TaxType,
            request.Region,
            request.ValidFrom
        );

        category.AddTaxRule(taxRule);
        
        await _unitOfWork.Repository<TaxRule>().AddAsync(taxRule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return taxRule.Id;
    }
}
