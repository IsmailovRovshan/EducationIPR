using System.Linq.Expressions;
using Dal;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.RepositoryInterfaces;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly DatabaseDbContext _dbContext;

    public OutboxMessageRepository(DatabaseDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddAsync(OutboxMessage outboxMessage)
    {
        _dbContext.OutboxMessages.Add(outboxMessage);
    }

    public async Task<List<OutboxMessage>> GetListAsync(Expression<Func<OutboxMessage, bool>> predicate = null)
    {
        var query = _dbContext.OutboxMessages.AsQueryable();
        
        if (predicate != null)
            query = query.Where(predicate);

        return await query.ToListAsync();
    }
}