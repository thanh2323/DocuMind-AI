using Xunit;
using Moq;
using DocuMind.Application.Services.UserService;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Application.Interface.IUser;
using DocuMind.Core.Entities;
using Microsoft.Extensions.Logging;
using DocuMind.Application.DTOs.User;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace DocuMind.Application.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IDocumentRepository> _mockDocRepo;
        private readonly Mock<IChatSessionRepository> _mockChatRepo;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockDocRepo = new Mock<IDocumentRepository>();
            _mockChatRepo = new Mock<IChatSessionRepository>();
            _mockLogger = new Mock<ILogger<UserService>>();

            _userService = new UserService(
                _mockUserRepo.Object,
                _mockDocRepo.Object,
                _mockChatRepo.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetProfile_ValidId_ReturnsProfileWithCounts()
        {
            // ARRANGE
            int userId = 1;
            var user = new User { Id = userId, FullName = "Test User", Email = "test@example.com", IsLocked = false };
            var chatSessions = new List<ChatSession> { new ChatSession(), new ChatSession() }; // 2 chats

            // Mock Repositories
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockDocRepo.Setup(repo => repo.CountUserDocumentsAsync(userId)).ReturnsAsync(5); // 5 docs
            _mockChatRepo.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(chatSessions);

            // ACT
            var result = await _userService.GetProfile(userId);

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal("Test User", result.Data.FullName);
            Assert.Equal(5, result.Data.TotalDocuments);
            Assert.Equal(2, result.Data.TotalChats);
        }

        [Fact]
        public async Task GetProfile_UserNotFound_ReturnsFailure()
        {
            // ARRANGE
            int userId = 99;
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // ACT
            var result = await _userService.GetProfile(userId);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task GetProfile_UserLocked_ReturnsFailure()
        {
            // ARRANGE
            int userId = 1;
            var user = new User { Id = userId, IsLocked = true };
            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _userService.GetProfile(userId);

            // ASSERT
            Assert.False(result.Success);
            Assert.Contains("locked", result.Message);
        }

        [Fact]
        public async Task UpdateProfile_ValidData_ReturnsSuccess()
        {
            // ARRANGE
            int userId = 1;
            var user = new User { Id = userId, FullName = "Old Name", IsLocked = false };
            var updateDto = new UpdateProfileDto { FullName = "New Name" };

            _mockUserRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // ACT
            var result = await _userService.UpdateProfile(userId, updateDto);

            // ASSERT
            Assert.True(result.Success);
            
            // VERIFY: Should update name to "New Name"
            _mockUserRepo.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.FullName == "New Name")), Times.Once);
            _mockUserRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
