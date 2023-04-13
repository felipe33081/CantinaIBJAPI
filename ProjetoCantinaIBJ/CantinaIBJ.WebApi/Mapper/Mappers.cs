using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Model;

namespace CantinaIBJ.WebApi.Mapper;

public class Mappers
{
    readonly IProductHistoricRepository _productHistoricRepository;
    public Mappers(
        IProductHistoricRepository productHistoricRepository) 
    { 
        _productHistoricRepository = productHistoricRepository;
    }

    public async Task ProductToProductHistoric(UserContext user, Product product)
    {
        ProductHistoric productHistoric = new()
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            Description = product.Description,
            Diponibility = product.Diponibility,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = user.GetCurrentUser()
        };
        await _productHistoricRepository.AddProductHistoricAsync(user, productHistoric);
    }
}