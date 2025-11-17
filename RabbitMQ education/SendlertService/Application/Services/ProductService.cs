using System.Text.Json;
using Dal;
using Dal.UnitOfWork;
using Domain.Models;
using Domain.RepositoryInterfaces;
using Services.Contracts;

namespace SendlertService.Services;

public class ProductService : IProductService
{
    private readonly DatabaseDbContext _dbContext;
    IProductRepository _productRepository;
    IUnitOfWork _unitOfWork;
    IOutboxMessageRepository _outboxMessageRepository;
    
    public ProductService(IProductRepository productRepository, 
        DatabaseDbContext dbContext, IUnitOfWork unitOfWork, 
        IOutboxMessageRepository outboxMessageRepository)
    {
        _productRepository = productRepository;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _outboxMessageRepository = outboxMessageRepository;
    }

    public async Task Add(Product product)
    {
        await _unitOfWork.BeginTransaction();
        
        try
        {
            await _productRepository.AddAsync(product);

            var outboxMessage = new OutboxMessage
            {
                OccurredOnUtc = DateTime.UtcNow,
                Type = nameof(Product),
                Payload = JsonSerializer.Serialize(product),
            };
                
            await _outboxMessageRepository.AddAsync(outboxMessage);
            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
        }
    }
}