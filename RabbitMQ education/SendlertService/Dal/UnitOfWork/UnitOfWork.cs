using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dal.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly DatabaseDbContext _dbContext;
    private IDbContextTransaction _transaction;
    private bool _disposed;
    
    public UnitOfWork(DatabaseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransaction()
    {
        if (_transaction != null) 
            return;
        
        _transaction = await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }
    
    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction == null) 
            return;

        await _dbContext.SaveChangesAsync(ct);
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
    
    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction == null) return;

        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;

        foreach (var entry in _dbContext.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        if (_transaction != null)
        {
            try { await _transaction.RollbackAsync(); } catch { /* swallow */ }
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        await _dbContext.DisposeAsync();
        _disposed = true;
    }
}