using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Document;
using DocuMind.Application.DTOs.User;
using DocuMind.Application.DTOs.User.Dashboard;
using DocuMind.Application.Interface.IUser;
using DocuMind.Core.Entities;
using DocuMind.Core.Enum;
using DocuMind.Core.Interfaces.IRepo;

namespace DocuMind.Application.Services.UserService
{
    public class DashBoardService : IUserDashboardService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly IUserRepository _userRepository;

        public DashBoardService(IUserRepository userRepository,IDocumentRepository documentRepository, IChatSessionRepository chatSessionRepository)
        {
            _documentRepository = documentRepository;
            _userRepository = userRepository;
            _chatSessionRepository = chatSessionRepository;
        }
        public async Task<ServiceResult<UserDashboardDto>> GetDashboardAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user!.IsLocked)
            {
                return ServiceResult<UserDashboardDto>.Fail("Your account has been locked. Please contact support.");
            }

            /*     // Get total documents and status counts
                 var totalDocsTask = _documentRepository.CountUserDocumentsAsync(id);
                 var statusTask = _documentRepository.GetStatusCountsAsync(id);


                 //  Get total chat sessions
                 var chatSessionsTask = _chatSessionRepository.GetByUserIdAsync(id);

                 // Get recent documents and chat sessions
                 var recentDocsTask = _documentRepository.GetRecentDocumentsAsync(id, 5);
                 var recentChatsTask = _chatSessionRepository.GetRecentChatsAsync(id, 5);

                 await Task.WhenAll(totalDocsTask, statusTask, chatSessionsTask, recentDocsTask, recentChatsTask);

                 var totalDocs = totalDocsTask.Result;
                 var statusMap = statusTask.Result;
                 var chatSessions = chatSessionsTask.Result;
                 var recentDocs = recentDocsTask.Result;
                 var recentChats = recentChatsTask.Result;*/

            var totalDocs = await _documentRepository.CountUserDocumentsAsync(id);
            var statusMap = await _documentRepository.GetStatusCountsAsync(id);
            var chatSessions = await _chatSessionRepository.GetByUserIdAsync(id);
            var recentDocs = await _documentRepository.GetRecentDocumentsAsync(id, 5);
            var recentChats = await _chatSessionRepository.GetRecentChatsAsync(id, 5);


            var dashboard = new UserDashboardDto
            {
                Statistics = new UserDashboardStatisticsDto
                {
                    TotalDocuments = totalDocs,
                    StatusCounts = statusMap,
                    TotalChats = chatSessions.Count()
                },
                RecentDocuments = recentDocs.Select(d => new DocumentItemDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt
                }),

                RecentChats = recentChats.Select(c => new SessionDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    LastActiveAt = c.LastActiveAt
                })
            };

            return ServiceResult<UserDashboardDto>.Ok(dashboard);
        }
    }
}
