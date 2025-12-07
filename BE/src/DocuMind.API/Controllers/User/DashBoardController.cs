using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.User.Dashboard;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using DocuMind.Application.Interface.IUser;

namespace DocuMind.API.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashBoardController : Controller
    {
        private readonly IUserDashboardService _dashboardService;
        public DashBoardController(IUserDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var dashboard = await _dashboardService.GetDashboardAsync(int.Parse(userId!));
            if (dashboard == null)
            {
                return BadRequest(ApiResponse<UserDashboardDto>.ErrorResponse(dashboard!.Message));
            }
            return Ok(ApiResponse<UserDashboardDto>.SuccessResponse(dashboard.Data!, dashboard.Message));
        }
    }
}
