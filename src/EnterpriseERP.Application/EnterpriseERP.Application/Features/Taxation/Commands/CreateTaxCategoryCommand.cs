using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Taxation;
using EnterpriseERP.SharedKernel.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Taxation.Commands;

public class CreateTaxCategoryCommand : IRequest<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateTaxCategoryCommandHandler : IRequestHandler<CreateTaxCategoryCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaxCategoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTaxCategoryCommand request, CancellationToken cancellationToken)
    {
        var taxCategory = new TaxCategory(request.Code, request.Name, request.Description);

        await _unitOfWork.Repository<TaxCategory>().AddAsync(taxCategory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return taxCategory.Id;
    }
}
