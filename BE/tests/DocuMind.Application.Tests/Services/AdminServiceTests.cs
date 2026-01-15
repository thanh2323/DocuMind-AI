using Xunit;
using Moq;
using DocuMind.Application.Services.AdminService;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IStorage;
using DocuMind.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DocuMind.Application.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IDocumentRepository> _mockDocRepo;
        private readonly Mock<IChatSessionRepository> _mockChatRepo;
        private readonly Mock<IStorageService> _mockStorageService;
        private readonly Mock<ILogger<AdminService>> _mockLogger;
        private readonly AdminService _adminService;

        public AdminServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockDocRepo = new Mock<IDocumentRepository>();
            _mockChatRepo = new Mock<IChatSessionRepository>();
            _mockStorageService = new Mock<IStorageService>();
            _mockLogger = new Mock<ILogger<AdminService>>();

            _adminService = new AdminService(
                _mockUserRepo.Object,
                _mockDocRepo.Object,
                _mockChatRepo.Object,
                _mockStorageService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task LockUser_ValidId_ReturnsSuccess()
        {
            // ARRANGE
            int userId = 1;
            var user = new User { Id = userId, IsLocked = false };
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _adminService.LockUser(userId);

            // ASSERT
            Assert.True(result.Success);
            
            // VERIFY
            _mockUserRepo.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.Id == userId && u.IsLocked == true)), Times.Once);
            _mockUserRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UnlockUser_ValidId_ReturnsSuccess()
        {
            // ARRANGE
            int userId = 1;
            var user = new User { Id = userId, IsLocked = true };
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _adminService.UnlockUser(userId);

            // ASSERT
            Assert.True(result.Success);

            // VERIFY
            _mockUserRepo.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.Id == userId && u.IsLocked == false)), Times.Once);
            _mockUserRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ValidId_ReturnsSuccess()
        {
            // ARRANGE
            int userId = 1;
            var user = new User { Id = userId };
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _adminService.DeleteUser(userId);

            // ASSERT
            Assert.True(result.Success);

            // VERIFY
            _mockUserRepo.Verify(repo => repo.DeleteAsync(user), Times.Once);
            _mockUserRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsUsersWithStats()
        {
            // ARRANGE
            var usersWithStats = new List<(User User, int DocumentCount, int ChatCount)>
            {
                (new User { Id = 1, FullName = "User 1" }, 5, 2),
                (new User { Id = 2, FullName = "User 2" }, 0, 0)
            };

            _mockUserRepo.Setup(repo => repo.GetUsersWithStatsAsync()).ReturnsAsync(usersWithStats);

            // ACT
            var result = await _adminService.GetAllUsers();

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count);
            
            // Check mappings
            Assert.Equal(5, result.Data[0].DocumentCount);
            Assert.Equal(2, result.Data[0].ChatCount);
            Assert.Equal("User 1", result.Data[0].FullName);
            
            Assert.Equal(0, result.Data[1].DocumentCount);
        }
    }
}
