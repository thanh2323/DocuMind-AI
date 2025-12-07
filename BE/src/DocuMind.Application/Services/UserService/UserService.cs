using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Auth;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.User;
using DocuMind.Application.DTOs.User.Dashboard;
using DocuMind.Application.Interface;
using DocuMind.Application.Interface.IUser;
using DocuMind.Core.Entities;
using DocuMind.Core.Interfaces.IRepo;
using Microsoft.Extensions.Logging;



namespace DocuMind.Application.Services.UserService

{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<UserService> _logger;
        public UserService(IUserRepository userRepository, IDocumentRepository documentRepository,IChatSessionRepository chatSessionRepository, ILogger<UserService> logger) 
        {
            _chatSessionRepository = chatSessionRepository;
            _documentRepository = documentRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public Task<ServiceResult<bool>> DeleteAccount(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<UserProfileDto>> GetProfile(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user!.IsLocked)
            {
                return ServiceResult<UserProfileDto>.Fail("Your account has been locked. Please contact support.");
            }
            // Get total documents
            var totalDocuments = await _documentRepository.CountUserDocumentsAsync(id);

            //  Get total chat sessions
            var chatSessions = await _chatSessionRepository.GetByUserIdAsync(id);
            var totalChats = chatSessions.Count();

            // Prepare profile DTO
            var returnDto = new UserProfileDto
            {
       
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                TotalDocuments = totalDocuments,
                TotalChats = totalChats
            };

            return ServiceResult<UserProfileDto>.Ok(returnDto);
        }

        public async Task<ServiceResult<UserProfileDto>> UpdateProfile(int id, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user!.IsLocked)
            {
                return ServiceResult<UserProfileDto>.Fail("Your account has been locked. Please contact support.");
            }

            // Update fields
            user.FullName = dto.FullName ?? user.FullName;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync(user);

            // Prepare profile DTO 
            var returnDto = new UserProfileDto
            {
    
                FullName = user.FullName,
            };

            return ServiceResult<UserProfileDto>.Ok(returnDto, "Profile updated successfully");
        }
    }
}
