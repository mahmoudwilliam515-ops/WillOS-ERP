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

public record TaxCategoryDto(Guid Id, string Code, string Name, string Description, bool IsActive);

public class GetTaxCategoriesQuery : IRequest<List<TaxCategoryDto>>
{
}

public class GetTaxCategoriesQueryHandler : IRequestHandler<GetTaxCategoriesQuery, List<TaxCategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTaxCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<TaxCategoryDto>> Handle(GetTaxCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Repository<TaxCategory>().GetAllAsync();
        
        var dtos = categories.Select(c => new TaxCategoryDto(
            c.Id,
            c.Code,
            c.Name,
            c.Description,
            c.IsActive
        )).ToList();

        return dtos;
    }
}
