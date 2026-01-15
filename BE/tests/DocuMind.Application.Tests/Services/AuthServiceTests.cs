using Xunit;
using Moq;
using DocuMind.Application.Services.AuthService;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IAuth;
using Microsoft.Extensions.Logging;
using DocuMind.Application.DTOs.Auth;
using DocuMind.Core.Entities;
using System.Threading.Tasks;

// Adjust namespace if needed based on project structure
namespace DocuMind.Application.Tests.Services
{
    public class AuthServiceTests
    {
        // 1. Define Mocks (Fake dependencies)
        // These replace the real database and external services
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<ILogger<AuthService>> _mockLogger;

        // 2. Define the System Under Test (SUT)
        // This is the actual class we want to test
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Initialize Mocks
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockJwtService = new Mock<IJwtService>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            // Initialize SUT with Mocks instead of real implementations
            _authService = new AuthService(
                _mockUserRepo.Object,
                _mockPasswordHasher.Object,
                _mockJwtService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsSuccessAndToken()
        {
            // ARRANGE
            // 1. Setup Input Data
            var loginDto = new LoginDto { Email = "test@example.com", Password = "password123" };
            var user = new User
            {
                Id = 1,
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                IsLocked = false,
                Role = "User"
            };

            // 2. Setup Mock Behavior
            // When repo.GetByEmailAsync is called with "test@example.com", return our fake user
            _mockUserRepo.Setup(repo => repo.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // When password verification happens, return true
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                .Returns(true);

            // When token is generated, return "fake_jwt_token"
            _mockJwtService.Setup(jwt => jwt.GenerateToken(user))
                .Returns("fake_jwt_token");

            // ACT 
            // Call the actual method
            var result = await _authService.LoginAsync(loginDto);

            // ASSERT (Kiểm tra)
            // Verify the results match expectations
            Assert.True(result.Success, "Login should be successful");
            Assert.NotNull(result.Data);
            Assert.Equal("fake_jwt_token", result.Data.Token);
            Assert.Equal(user.Email, result.Data.Email);
        }
        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsFailure()
        {
            // ARRANGE
            // 1. Setup Input Data
            var loginDto = new LoginDto { Email = "test@example.com", Password = "wrong_password" };
            var user = new User
            {
                Id = 1,
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                IsLocked = false,
                Role = "User"
            };

            // 2. Setup Mock Behavior
            _mockUserRepo.Setup(repo => repo.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                .Returns(false);

            // ACT 
            var result = await _authService.LoginAsync(loginDto);

            // ASSERT
            Assert.False(result.Success, "Login should fail");
            Assert.Equal("Invalid email or password", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task LoginAsync_UserLocked_ReturnsFailure()
        {
            // ARRANGE
            var loginDto = new LoginDto { Email = "locked@example.com", Password = "password123" };
            var user = new User
            {
                Id = 2,
                FullName = "Locked User",
                Email = "locked@example.com",
                PasswordHash = "hashed_password",
                IsLocked = true, // KEY POINT: User is locked
                Role = "User"
            };

            _mockUserRepo.Setup(repo => repo.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                .Returns(true); // Password is correct, but still fail

            // ACT
            var result = await _authService.LoginAsync(loginDto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("Your account has been locked. Please contact support.", result.Message);
        }
        [Fact]
        public async Task RegisterAsync_ValidData_ReturnsSuccess()
        {
            // ARRANGE
            var registerDto = new RegisterDto { FullName = "New User", Email = "new@example.com", Password = "password123" };
            
            // Mock: Email does NOT exist
            _mockUserRepo.Setup(repo => repo.EmailExistsAsync(registerDto.Email))
                .ReturnsAsync(false);

            // Mock: Hash password
            _mockPasswordHasher.Setup(hasher => hasher.HashPassword(registerDto.Password))
                .Returns("hashed_password");

            // Mock: Generate Token
            _mockJwtService.Setup(jwt => jwt.GenerateToken(It.IsAny<User>()))
                .Returns("fake_register_token");

            // ACT
            var result = await _authService.RegisterAsync(registerDto);

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal("fake_register_token", result.Data!.Token);

            // VERIFY (Kiểm tra hành vi)
            // Verify that AddAsync and SaveChangesAsync were actually called
            _mockUserRepo.Verify(repo => repo.AddAsync(It.Is<User>(u => u.Email == registerDto.Email)), Times.Once);
            _mockUserRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_EmailExists_ReturnsFailure()
        {
            // ARRANGE
            var registerDto = new RegisterDto { FullName = "Duplicate User", Email = "existing@example.com", Password = "password123" };

            // Mock: Email DOES exist
            _mockUserRepo.Setup(repo => repo.EmailExistsAsync(registerDto.Email))
                .ReturnsAsync(true);

            // ACT
            var result = await _authService.RegisterAsync(registerDto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("Email is already registered", result.Message);

            // VERIFY
            // Verify that AddAsync was NEVER called (important!)
            _mockUserRepo.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        }
        [Theory]
        [InlineData("", "password")] // Empty Email
        [InlineData("test@example.com", "")] // Empty Password
        public async Task LoginAsync_InvalidInput_ReturnsFailure(string email, string password)
        {
            // ARRANGE
            var loginDto = new LoginDto { Email = email, Password = password };

            // ACT
            var result = await _authService.LoginAsync(loginDto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("Invalid request", result.Message);
        }

        [Theory]
        [InlineData("", "password", "test name")] // Empty Email
        [InlineData("test@example.com", "", "test name")] // Empty Password
        [InlineData("test@example.com", "password", "")] // Empty FullName
        public async Task RegisterAsync_InvalidInput_ReturnsFailure(string email, string password, string fullName)
        {
            // ARRANGE
            var registerDto = new RegisterDto { Email = email, Password = password, FullName = fullName };

            // ACT
            var result = await _authService.RegisterAsync(registerDto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("Invalid request", result.Message);
        }
        [Fact]
        public async Task ChangePasswordAsync_ValidRequest_ReturnsSuccess()
        {
            // ARRANGE
            var changeDto = new ChangePasswordDto { Email = "test@example.com", CurrentPassword = "old_password", NewPassword = "new_password" };
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = "hashed_old_password",
                IsLocked = false
            };

            _mockUserRepo.Setup(repo => repo.GetByEmailAsync(changeDto.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(changeDto.CurrentPassword, user.PasswordHash)).Returns(true);
            _mockPasswordHasher.Setup(hasher => hasher.HashPassword(changeDto.NewPassword)).Returns("hashed_new_password");
            _mockJwtService.Setup(jwt => jwt.GenerateToken(user)).Returns("new_token");

            // ACT
            var result = await _authService.ChangePasswordAsync(changeDto);

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal("new_token", result.Data!.Token);
            
            // VERIFY: Should update user and save changes
            _mockUserRepo.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.PasswordHash == "hashed_new_password")), Times.Once);
            _mockUserRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_UserNotFound_ReturnsFailure()
        {
            // ARRANGE
            var changeDto = new ChangePasswordDto { Email = "unknown@example.com", CurrentPassword = "any", NewPassword = "any" };
            _mockUserRepo.Setup(repo => repo.GetByEmailAsync(changeDto.Email)).ReturnsAsync((User?)null);

            // ACT
            var result = await _authService.ChangePasswordAsync(changeDto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_WrongCurrentPassword_ReturnsFailure()
        {
            // ARRANGE
            var changeDto = new ChangePasswordDto { Email = "test@example.com", CurrentPassword = "wrong_old", NewPassword = "new" };
            var user = new User { Email = "test@example.com", PasswordHash = "hashed_old" };

            _mockUserRepo.Setup(repo => repo.GetByEmailAsync(changeDto.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(changeDto.CurrentPassword, user.PasswordHash)).Returns(false);

            // ACT
            var result = await _authService.ChangePasswordAsync(changeDto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("Current password is incorrect", result.Message);
            
            // VERIFY: Should NOT update anything
            _mockUserRepo.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Theory]
        [InlineData("", "old", "new")] // Empty Email
        [InlineData("test@example.com", "", "new")] // Empty Current
        [InlineData("test@example.com", "old", "")] // Empty New
        public async Task ChangePasswordAsync_InvalidInput_ReturnsFailure(string email, string current, string newPass)
        {
            // ARRANGE
            var changeDto = new ChangePasswordDto { Email = email, CurrentPassword = current, NewPassword = newPass };

            // ACT
            var result = await _authService.ChangePasswordAsync(changeDto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("Invalid request", result.Message);
        }
    }
}
