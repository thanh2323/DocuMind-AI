using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DocuMind.API.Controllers.Auth;
using DocuMind.Application.Interface.IAuth;
using DocuMind.Application.DTOs.Auth;
using DocuMind.Application.DTOs.Common;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;

namespace DocuMind.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Register_ValidInput_ReturnsOk()
        {
            // ARRANGE
            var registerDto = new RegisterDto { FullName = "New User", Email = "new@example.com", Password = "password" };
            var authResponse = new AuthResponseDto { Token = "register-token", FullName = "New User" };

            // Mock Service Success
            _mockAuthService.Setup(s => s.RegisterAsync(registerDto))
                .ReturnsAsync(ServiceResult<AuthResponseDto>.Ok(authResponse));

            // ACT
            var result = await _controller.Register(registerDto);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<AuthResponseDto>>(okResult.Value);

            Assert.True(apiResponse.Success);
            Assert.Equal("register-token", apiResponse.Data!.Token);
        }

        [Fact]
        public async Task Register_EmailExists_ReturnsBadRequest()
        {
            // ARRANGE
            var registerDto = new RegisterDto { FullName = "New User", Email = "existing@example.com", Password = "password" };
            
            // Mock Service Failure
            _mockAuthService.Setup(s => s.RegisterAsync(registerDto))
                .ReturnsAsync(ServiceResult<AuthResponseDto>.Fail("Email is already registered"));

            // ACT
            var result = await _controller.Register(registerDto);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<AuthResponseDto>>(badRequestResult.Value);

            Assert.False(apiResponse.Success);
            Assert.Equal("Email is already registered", apiResponse.Message);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // ARRANGE
            var loginDto = new LoginDto { Email = "test@example.com", Password = "password" };
            var authResponse = new AuthResponseDto { Token = "dummy-token", FullName = "Test User" };
            
            // Mock Service Success
            _mockAuthService.Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync(ServiceResult<AuthResponseDto>.Ok(authResponse));

            // ACT
            var result = await _controller.Login(loginDto);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<AuthResponseDto>>(okResult.Value);
            
            Assert.True(apiResponse.Success);
            Assert.Equal("dummy-token", apiResponse.Data!.Token);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsBadRequest()
        {
            // ARRANGE
            var loginDto = new LoginDto { Email = "test@example.com", Password = "wrong" };
            
            // Mock Service Failure
            _mockAuthService.Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync(ServiceResult<AuthResponseDto>.Fail("Invalid password"));

            // ACT
            var result = await _controller.Login(loginDto);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<AuthResponseDto>>(badRequestResult.Value);
            
            Assert.False(apiResponse.Success);
            Assert.Equal("Invalid password", apiResponse.Message);
        }

        [Fact]
        public async Task ChangePassword_Authorized_ReturnsOk()
        {
            // ARRANGE
            var changeDto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" };
            var email = "user@example.com";

            // Mock User Identity (HttpContext)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, email)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Mock Service Success
            _mockAuthService.Setup(s => s.ChangePasswordAsync(It.Is<ChangePasswordDto>(d => d.Email == email)))
                .ReturnsAsync(ServiceResult<AuthResponseDto>.Ok(new AuthResponseDto()));

            // ACT
            var result = await _controller.ChangePassword(changeDto);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<AuthResponseDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
        }

        [Fact]
        public async Task ChangePassword_Unauthorized_Returns401()
        {
            // ARRANGE
            var changeDto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "new" };

            // Mock Empty User context (Not logged in)
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() // User is null or default
            };

            // ACT
            var result = await _controller.ChangePassword(changeDto);

            // ASSERT
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
