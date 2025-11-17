using Domain.Models;

namespace Domain.RepositoryInterfaces
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
    }
}
