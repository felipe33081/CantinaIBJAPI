using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Model.Product;

namespace CantinaIBJ.WebApi.Mapper;

public class Mappers
{
    readonly IProductHistoricRepository _productHistoricRepository;
    public Mappers(
        IProductHistoricRepository productHistoricRepository) 
    { 
        _productHistoricRepository = productHistoricRepository;
    }

    public async Task ProductToProductHistoric(Product product)
    {
        ProductHistoric productHistoric = new()
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            Description = product.Description,
            Diponibility = product.Diponibility,
            UpdatedAt = DateTime.UtcNow
        };
        await _productHistoricRepository.AddProductHistoricAsync(productHistoric);
    }
}