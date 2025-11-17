using Domain.Models;

namespace Services.Contracts;

public interface IProductService
{
    Task Add(Product product);
}