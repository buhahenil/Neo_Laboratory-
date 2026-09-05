using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Leb.Core.Interfaces;

namespace Leb.API.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] int? branchId = null,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var stats = await _dashboardRepository.GetAdminDashboardStatsAsync(branchId, year, month, startDate, endDate);
            return Ok(stats);
        }
    }
}
