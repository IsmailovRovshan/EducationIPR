namespace Dal.UnitOfWork;

public interface IUnitOfWork
{
    Task BeginTransaction();
    Task<int> SaveChangesAsync();
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    ValueTask DisposeAsync();
}