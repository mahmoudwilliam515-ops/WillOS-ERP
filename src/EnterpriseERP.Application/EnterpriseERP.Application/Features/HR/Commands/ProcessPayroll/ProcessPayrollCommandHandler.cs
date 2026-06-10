using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.Application.Common.Interfaces.Repositories; 
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting; 
using EnterpriseERP.Domain.Entities.HR; 
using MediatR;  

namespace EnterpriseERP.Application.Features.HR.Commands.ProcessPayroll;  

public class ProcessPayrollCommandHandler : IRequestHandler<ProcessPayrollCommand, Guid> 
{     
    private readonly IGenericRepository<Employee> _employeeRepo;     
    private readonly IGenericRepository<PayrollTransaction> _payrollRepo;     
    private readonly IGenericRepository<JournalEntry> _journalRepo;     
    private readonly IGenericRepository<EmployeeDeduction> _deductionRepo;     
    private readonly IUnitOfWork _unitOfWork;      
    private readonly IAccountingPostingService _postingService;
    private readonly ICurrentUserService _currentUserService;
    
    public ProcessPayrollCommandHandler(         
        IGenericRepository<Employee> employeeRepo,         
        IGenericRepository<PayrollTransaction> payrollRepo,         
        IGenericRepository<JournalEntry> journalRepo,         
        IGenericRepository<EmployeeDeduction> deductionRepo,         
        IUnitOfWork unitOfWork,
        IAccountingPostingService postingService,
        ICurrentUserService currentUserService)     
    {         
        _employeeRepo = employeeRepo;         
        _payrollRepo = payrollRepo;         
        _journalRepo = journalRepo;         
        _deductionRepo = deductionRepo;         
        _unitOfWork = unitOfWork;     
        _postingService = postingService;
        _currentUserService = currentUserService;
    }      
    
    public async Task<Guid> Handle(ProcessPayrollCommand request, CancellationToken cancellationToken)     
    {         
        var companyId = _currentUserService.CompanyId;
        if (companyId == null) throw new HRDomainException("Company context is required for payroll.");

        var (activeEmployeesList, _) = await _employeeRepo.GetPagedAsync(1, 1000, e => e.IsActive && e.CompanyId == companyId);          
        var activeEmployees = activeEmployeesList.ToList();

        if (!activeEmployees.Any())
        {
            throw new HRDomainException("No active employees found for payroll processing.");
        }

        var periodStart = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);         
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);          
        
        var pendingDeductions = await _deductionRepo.FindAsync(             
            d => d.Year == request.Year && d.Month == request.Month && !d.IsApplied);         
        var deductionsByEmployee = pendingDeductions             
            .GroupBy(d => d.EmployeeId)             
            .ToDictionary(g => g.Key, g => g.ToList());          
            
        decimal totalBasic = 0;         
        decimal totalHousing = 0;         
        decimal totalTransport = 0;         
        decimal totalDeductions = 0;         
        decimal totalNet = 0;           
        var payrollTransactions = new List<PayrollTransaction>();         
        var appliedDeductions = new List<EmployeeDeduction>();          
        
        foreach (var emp in activeEmployees)         
        {             
            var grossSalary = emp.BasicSalary + emp.HousingAllowance + emp.TransportationAllowance;              
            decimal empDeductions = 0;             
            if (deductionsByEmployee.TryGetValue(emp.Id, out var empDeductionList))             
            {                 
                empDeductions = empDeductionList.Sum(d => d.Amount);                 
                appliedDeductions.AddRange(empDeductionList);             
            }              
            
            var netSalary = grossSalary - empDeductions;             
            if (netSalary < 0) netSalary = 0; 
            
            totalBasic      += emp.BasicSalary;             
            totalHousing    += emp.HousingAllowance;             
            totalTransport  += emp.TransportationAllowance;             
            totalDeductions += empDeductions;             
            totalNet        += netSalary;              
            
            payrollTransactions.Add(new PayrollTransaction             
            {                 
                EmployeeId         = emp.Id,                 
                PayrollPeriodStart = periodStart,                 
                PayrollPeriodEnd   = periodEnd,                 
                PaymentDate        = DateTime.UtcNow,                 
                BasicSalary        = emp.BasicSalary,                 
                Allowances         = emp.HousingAllowance + emp.TransportationAllowance,                 
                Deductions         = empDeductions,                 
                NetSalary          = netSalary,                 
                IsProcessed        = true              
            });         
        }          
        
        var lines = new List<JournalEntryLine>         
        {             
            new JournalEntryLine             
            {                 
                AccountCode = "6101", AccountName = "Salaries Expense",                 
                DebitAmount = totalBasic, CreditAmount = 0,                 
                Description = "Basic Salaries"             
            },             
            new JournalEntryLine             
            {                 
                AccountCode = "6102", AccountName = "Housing Allowance Expense",                 
                DebitAmount = totalHousing, CreditAmount = 0,                 
                Description = "Housing Allowances"             
            },             
            new JournalEntryLine             
            {                 
                AccountCode = "6103", AccountName = "Transportation Allowance Expense",                 
                DebitAmount = totalTransport, CreditAmount = 0,                 
                Description = "Transport Allowances"             
            },             
            new JournalEntryLine             
            {                 
                AccountCode = "2101", AccountName = "Accrued Salaries Payable",                 
                DebitAmount = 0, CreditAmount = totalNet,                 
                Description = $"Net Salaries Payable {request.Month}/{request.Year}"             
            }         
        };          
        
        if (totalDeductions > 0)         
        {             
            lines.Add(new JournalEntryLine             
            {                 
                AccountCode = "2102", AccountName = "Employee Deductions Payable",                 
                DebitAmount = 0, CreditAmount = totalDeductions,                 
                Description = $"Employee Deductions {request.Month}/{request.Year}"             
            });         
        }          
        
        var grossTotal = totalBasic + totalHousing + totalTransport;          
        var journalEntry = new JournalEntry         
        {             
            CompanyId       = companyId.Value,
            EntryDate       = DateTime.UtcNow,             
            EntryNumber     = $"PR-{request.Year}{request.Month:D2}",             
            Description     = $"Payroll processing for {request.Month}/{request.Year}",             
            Status          = JournalEntryStatus.Posted,             
            ReferenceType   = "Payroll",             
            ReferenceNumber = $"{request.Year}-{request.Month}",             
            TotalDebit      = grossTotal,             
            TotalCredit     = grossTotal,             
            Lines           = lines         
        };          
        
        foreach (var line in lines) { line.CompanyId = companyId.Value; }

        await _unitOfWork.BeginTransactionAsync();         
        try         
        {             
            await _journalRepo.AddAsync(journalEntry);             
            await _unitOfWork.SaveChangesAsync(cancellationToken);              
            
            foreach (var tx in payrollTransactions)             
            {                 
                tx.JournalEntryId = journalEntry.Id;                 
                await _payrollRepo.AddAsync(tx);             
            }              
            
            foreach (var d in appliedDeductions)             
            {                 
                d.IsApplied = true;                 
                _deductionRepo.Update(d);             
            }              
            
            journalEntry.AddDomainEvent(new EnterpriseERP.Application.Features.HR.Events.PayrollProcessedEvent(                 
                request.Year, request.Month, activeEmployees.Count, totalNet, totalDeductions));               
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);             
            await _unitOfWork.CommitTransactionAsync();         
        }         
        catch         
        {             
            await _unitOfWork.RollbackTransactionAsync();             
            throw;         
        }          
        return journalEntry.Id;     
    } 
}
