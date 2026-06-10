using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.HR;
using MediatR;

namespace EnterpriseERP.Application.Features.HR.Commands.CreateEmployee;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Guid>
{
    private readonly IGenericRepository<Employee> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(IGenericRepository<Employee> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            EmployeeNumber = request.EmployeeNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            JobTitle = request.JobTitle,
            Department = request.Department,
            BasicSalary = request.BasicSalary,
            HousingAllowance = request.HousingAllowance,
            TransportationAllowance = request.TransportationAllowance,
            HireDate = request.HireDate,
            BranchId = request.BranchId,
            IsActive = true
        };

        await _repository.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}
