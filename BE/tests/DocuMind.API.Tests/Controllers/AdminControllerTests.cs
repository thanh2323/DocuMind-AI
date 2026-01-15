using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DocuMind.API.Controllers.Admin;
using DocuMind.Application.DTOs.Admin;
using DocuMind.Application.Interface.IAdmin;
using DocuMind.Application.DTOs.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DocuMind.API.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IAdminService> _mockAdminService;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockAdminService = new Mock<IAdminService>();
            _controller = new AdminController(_mockAdminService.Object);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOk()
        {
            // ARRANGE
            var users = new List<UserAdminDto> 
            { 
                new UserAdminDto { Id = 1, FullName = "User 1" },
                new UserAdminDto { Id = 2, FullName = "User 2" }
            };

            _mockAdminService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(ServiceResult<List<UserAdminDto>>.Ok(users));

            // ACT
            var result = await _controller.GetAllUsers();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<ServiceResult<List<UserAdminDto>>>(okResult.Value);
            Assert.True(returnedResult.Success);
            Assert.Equal(2, returnedResult.Data!.Count);
        }

        [Fact]
        public async Task GetAllUsers_ServiceFailure_ReturnsBadRequest()
        {
            // ARRANGE
            _mockAdminService.Setup(s => s.GetAllUsers())
                .ReturnsAsync(ServiceResult<List<UserAdminDto>>.Fail("Failed to fetch"));

            // ACT
            var result = await _controller.GetAllUsers();

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedResult = Assert.IsType<ServiceResult<List<UserAdminDto>>>(badRequestResult.Value);
            Assert.False(returnedResult.Success);
            Assert.Equal("Failed to fetch", returnedResult.Message);
        }

        [Fact]
        public async Task LockUser_ReturnsOk()
        {
            // ARRANGE
            int userId = 1;
            _mockAdminService.Setup(s => s.LockUser(userId))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // ACT
            var result = await _controller.LockUser(userId);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<ServiceResult<bool>>(okResult.Value);
            Assert.True(returnedResult.Success);
        }

        [Fact]
        public async Task LockUser_ServiceFail_ReturnsBadRequest()
        {
            // ARRANGE
            int userId = 1;
            _mockAdminService.Setup(s => s.LockUser(userId))
                .ReturnsAsync(ServiceResult<bool>.Fail("User not found"));

            // ACT
            var result = await _controller.LockUser(userId);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedResult = Assert.IsType<ServiceResult<bool>>(badRequestResult.Value);
            Assert.False(returnedResult.Success);
            Assert.Equal("User not found", returnedResult.Message);
        }

        [Fact]
        public async Task UnlockUser_ReturnsOk()
        {
            // ARRANGE
            int userId = 1;
            _mockAdminService.Setup(s => s.UnlockUser(userId))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // ACT
            var result = await _controller.UnlockUser(userId);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<ServiceResult<bool>>(okResult.Value);
            Assert.True(returnedResult.Success);
        }

        [Fact]
        public async Task UnlockUser_ServiceFail_ReturnsBadRequest()
        {
            // ARRANGE
            int userId = 1;
            _mockAdminService.Setup(s => s.UnlockUser(userId))
                .ReturnsAsync(ServiceResult<bool>.Fail("Error"));

            // ACT
            var result = await _controller.UnlockUser(userId);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedResult = Assert.IsType<ServiceResult<bool>>(badRequestResult.Value);
            Assert.False(returnedResult.Success);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOk()
        {
            // ARRANGE
            int userId = 1;
            _mockAdminService.Setup(s => s.DeleteUser(userId))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // ACT
            var result = await _controller.DeleteUser(userId);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResult = Assert.IsType<ServiceResult<bool>>(okResult.Value);
            Assert.True(returnedResult.Success);
        }

        [Fact]
        public async Task DeleteUser_ServiceFail_ReturnsBadRequest()
        {
            // ARRANGE
            int userId = 1;
            _mockAdminService.Setup(s => s.DeleteUser(userId))
                .ReturnsAsync(ServiceResult<bool>.Fail("Cannot delete admin"));

            // ACT
            var result = await _controller.DeleteUser(userId);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedResult = Assert.IsType<ServiceResult<bool>>(badRequestResult.Value);
            Assert.False(returnedResult.Success);
            Assert.Equal("Cannot delete admin", returnedResult.Message);
        }
    }
}
