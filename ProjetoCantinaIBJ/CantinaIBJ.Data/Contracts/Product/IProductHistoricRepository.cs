using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;

namespace CantinaIBJ.Data.Contracts;

public interface IProductHistoricRepository : IRepositoryBase<ProductHistoric>
{
    Task<IEnumerable<ProductHistoric>> GetProductHistorics(UserContext contextUser); 
    Task<ProductHistoric> GetProductHistoricByIdAsync(UserContext contextUser, int id); 
    Task AddProductHistoricAsync(UserContext contextUser, ProductHistoric productHistoric);
}