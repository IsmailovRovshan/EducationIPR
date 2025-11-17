using Dal;

namespace Domain.RepositoryInterfaces;

public class UnitOfWork
{
    private readonly DatabaseDbContext _dbContext;
    
    public UnitOfWork(DatabaseDbContext database)
    {
        _dbContext = database;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}