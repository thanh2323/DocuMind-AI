using DocuMind.Application.DTOs.Admin;
using DocuMind.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocuMind.Application.Interface.IAdmin;

namespace DocuMind.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var result = await _adminService.GetDashboardStats();
            if(!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _adminService.GetAllUsers();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _adminService.DeleteUser(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("users/{id}/lock")]
        public async Task<IActionResult> LockUser(int id)
        {
            var result = await _adminService.LockUser(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("users/{id}/unlock")]
        public async Task<IActionResult> UnlockUser(int id)
        {
            var result = await _adminService.UnlockUser(id);
             if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
