using DocuMind.Core.Entities;
using DocuMind.Application.DTOs.Auth;
using DocuMind.Application.Interface.IAuth;
using DocuMind.Core.Interfaces.IAuth;
using DocuMind.Core.Interfaces.IRepo;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.User;

namespace DocuMind.Application.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtService jwtService, ILogger<AuthService> logger)
        {
            this.userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _logger = logger;
        }
        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return ServiceResult<AuthResponseDto>.Fail("Invalid request");

            var user = await userRepository.GetByEmailAsync(dto.Email);

            if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                return ServiceResult<AuthResponseDto>.Fail("Invalid email or password");

            // Check if user is banned 
            if (user.IsLocked)
                return ServiceResult<AuthResponseDto>.Fail("Your account has been locked. Please contact support.");

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            _logger.LogInformation("User logged in: {Email}", user.Email);

            var returnDto = new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };
            return ServiceResult<AuthResponseDto>.Ok(returnDto, "login successful");
        }

        public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FullName))
                return ServiceResult<AuthResponseDto>.Fail("Invalid request");

            var existingUser = await userRepository.EmailExistsAsync(dto.Email);

            if (existingUser)
                return ServiceResult<AuthResponseDto>.Fail("Email is already registered");

            // Create new user
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = "User",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Registering new user: {Email}", user.Email);
            

            await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            _logger.LogInformation("User registered: {Email}", user.Email);

            var returnDto = new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            
            return ServiceResult<AuthResponseDto>.Ok(returnDto, "Registration successful");

        }

        public async Task<ServiceResult<AuthResponseDto>> ChangePasswordAsync(ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return ServiceResult<AuthResponseDto>.Fail("Invalid request");

            var user = await userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                return ServiceResult<AuthResponseDto>.Fail("User not found");


            // Verify current password
            if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return ServiceResult<AuthResponseDto>.Fail("Current password is incorrect");

            // Update to new password
            user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);

            _logger.LogInformation("Changing password for user: {Email}", user.Email);

           
            await userRepository.UpdateAsync(user);
            await userRepository.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            _logger.LogInformation("Password changed for user: {Email}", user.Email);

            var returnDto = new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            return ServiceResult<AuthResponseDto>.Ok(returnDto, "Change password successful");
        }
    }
}
