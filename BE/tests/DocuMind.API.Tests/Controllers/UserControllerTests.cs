using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DocuMind.API.Controllers.User;
using DocuMind.Application.Interface.IUser;
using DocuMind.Application.DTOs.User;
using DocuMind.Application.DTOs.Common;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using DocuMind.Application.Services.UserService;

namespace DocuMind.API.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        private void SetupUserContext(string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetProfile_Authenticated_ReturnsOk()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            var profile = new UserProfileDto { FullName = "Test User", Email = "test@example.com" };

            _mockUserService.Setup(s => s.GetProfile(1))
                .ReturnsAsync(ServiceResult<UserProfileDto>.Ok(profile));

            // ACT
            var result = await _controller.GetProfile();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<UserProfileDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal("Test User", apiResponse.Data.FullName);
        }

        [Fact]
        public async Task GetProfile_UserNotFound_ReturnsBadRequest()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            
            // Mock returning null or failure equivalent
            // Note: Controller checks if user == null OR result.Success.
            // Based on Controller code: if (user == null) return BadRequest.
            // Service returns ServiceResult<UserProfileDto>.
            
            _mockUserService.Setup(s => s.GetProfile(1))
                .ReturnsAsync((ServiceResult<UserProfileDto>?)null);

            // ACT
            var result = await _controller.GetProfile();

            // ASSERT
            // Controller handles null return explicitly
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // Verify error message if possible, or just the type
        }

        [Fact]
        public async Task UpdateProfile_Authenticated_ReturnsOk()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            var dto = new UpdateProfileDto { FullName = "Updated Name" };
            var updatedProfile = new UserProfileDto { FullName = "Updated Name" };

            _mockUserService.Setup(s => s.UpdateProfile(1, dto))
                .ReturnsAsync(ServiceResult<UserProfileDto>.Ok(updatedProfile));

            // ACT
            var result = await _controller.UpdateProfile(dto);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<UserProfileDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal("Updated Name", apiResponse.Data.FullName);
        }

        [Fact]
        public async Task UpdateProfile_ServiceFail_ReturnsBadRequest()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            var dto = new UpdateProfileDto { FullName = "Updated Name" };

            _mockUserService.Setup(s => s.UpdateProfile(1, dto))
                .ReturnsAsync(ServiceResult<UserProfileDto>.Fail("Update failed"));

            // ACT
            var result = await _controller.UpdateProfile(dto);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<UserProfileDto>>(badRequestResult.Value);
            Assert.False(apiResponse.Success);
            Assert.Equal("Update failed", apiResponse.Message);
        }
    }
}
