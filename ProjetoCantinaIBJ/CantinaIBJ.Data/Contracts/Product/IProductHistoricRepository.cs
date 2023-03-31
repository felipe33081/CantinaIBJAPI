using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model.Product;

namespace CantinaIBJ.Data.Contracts;

public interface IProductHistoricRepository : IRepositoryBase<ProductHistoric>
{
    Task<IEnumerable<ProductHistoric>> GetProductHistorics();
    Task<ProductHistoric> GetProductHistoricByIdAsync(int id);
    Task AddProductHistoricAsync(ProductHistoric productHistoric);
}