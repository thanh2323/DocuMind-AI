using System.Security.Claims;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Auth;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.User;
using DocuMind.Application.DTOs.User.Dashboard;
using DocuMind.Application.Interface.IUser;

using DocuMind.Application.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuMind.API.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
       

        public UserController(IUserService userService)
        {
         
            _userService = userService;
       
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
           var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userService.GetProfile(int.Parse(userId!));
            if (user == null)
            {
                return BadRequest(ApiResponse<UserService>.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(user.Data!, user.Message));
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _userService.UpdateProfile(int.Parse(userId!),dto);
            if (!result.Success)
                return BadRequest(ApiResponse<UserProfileDto>.ErrorResponse(result.Message));
            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(result.Data!, result.Message));
        }

      
    }
}
