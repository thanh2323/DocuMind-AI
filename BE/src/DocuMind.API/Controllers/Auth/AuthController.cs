using System.Security.Claims;
using Azure;
using DocuMind.Application.DTOs.Auth;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.Interface.IAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DocuMind.API.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.Success)
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(result.Message));


            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result.Data!, result.Message));

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(result.Message));

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result.Data!, result.Message));
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null) return Unauthorized(new { message = "You don't have account" });

            dto.Email = email;
            var result = await _authService.ChangePasswordAsync(dto);
            if (!result.Success)
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(result.Message));

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result.Data!, result.Message));
        }
    }
}

