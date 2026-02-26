using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;

namespace CantinaIBJ.Data.Contracts;

public interface IProductHistoricRepository : IRepositoryBase<ProductHistoric>
{
    Task<List<ProductHistoric>> GetProductHistorics(); 
    Task<ProductHistoric> GetProductHistoricByIdAsync(int id); 
    Task AddProductHistoricAsync(ProductHistoric productHistoric);
}