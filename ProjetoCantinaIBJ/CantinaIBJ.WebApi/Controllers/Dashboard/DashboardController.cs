using CantinaIBJ.Data.Contracts.Dashboard;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Controllers.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CantinaIBJ.WebApi.Common.Constants;

namespace CantinaIBJ.WebApi.Controllers.Dashboard
{
    public class DashboardController : CoreController
    {
        readonly IDashboardRepository _repository;

        public DashboardController(IDashboardRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("Metrics")]
        [Authorize(Policy.USER)]
        public IActionResult GetDashboardData([FromQuery] DateTime? initialDate = null, [FromQuery] DateTime? finalDate = null)
        {
            DateTime from = DateTime.Today;
            DateTime to = DateTime.Today.AddDays(1).AddTicks(-1);

            var data = _repository.GetDashboardDataAsync(initialDate ?? from, finalDate ?? to);

            return Ok(data);
        }
    }
}
