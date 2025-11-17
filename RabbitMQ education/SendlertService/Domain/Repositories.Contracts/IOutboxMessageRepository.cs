using System.Linq.Expressions;
using Domain.Models;

namespace Domain.RepositoryInterfaces;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage outboxMessage);
    Task<List<OutboxMessage>> GetListAsync(Expression<Func<OutboxMessage, bool>> predicate = null);
}