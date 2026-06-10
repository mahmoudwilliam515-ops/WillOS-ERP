using System.Collections.Concurrent;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace EnterpriseERP.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private readonly ConcurrentDictionary<Type, object> _repositories;
    private readonly EnterpriseERP.SharedKernel.DomainEvents.IDomainEventDispatcher _domainEventDispatcher;

    public UnitOfWork(ApplicationDbContext context, EnterpriseERP.SharedKernel.DomainEvents.IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _repositories = new ConcurrentDictionary<Type, object>();
        _domainEventDispatcher = domainEventDispatcher;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(GenericRepository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(type), _context);
            
            if (repositoryInstance != null)
            {
                _repositories.TryAdd(type, repositoryInstance);
            }
        }

        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null || _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await DispatchEvents();
            await SaveChangesAsync();
            await DispatchEvents(); // Catch events added during SaveChanges

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    private async Task DispatchEvents()
    {
        var domainEntities = _context.ChangeTracker
            .Entries<EnterpriseERP.SharedKernel.Common.BaseEntity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        if (domainEntities.Any())
        {
            await _domainEventDispatcher.DispatchAndClearEvents(domainEntities);
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        _currentTransaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}
