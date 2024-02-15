using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.Model;

namespace CantinaIBJ.Data.Contracts.Dashboard;

public interface IDashboardRepository
{
    DashboardData GetDashboardDataAsync(DateTime? from = null, DateTime? to = null);
}