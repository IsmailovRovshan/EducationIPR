using Dal;
using Domain.Models;

namespace Domain.RepositoryInterfaces;

public class ProductRepository : IProductRepository
{
    private readonly DatabaseDbContext _dbContext;

    public ProductRepository(DatabaseDbContext database)
    {
        _dbContext = database;
    }

    public async Task AddAsync(Product product)
    {
        _dbContext.Products.Add(product);
    }
}
