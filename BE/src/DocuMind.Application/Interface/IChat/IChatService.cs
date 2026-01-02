using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Chat;

namespace DocuMind.Application.Interface.IChat
{
   public interface IChatService
    {
        Task<ServiceResult<SessionDto>> CreateChatAsync(int userId, CreateSessionDto dto);
        Task<ServiceResult<List<SessionDto>>> GetSessionsAsync(int userId);
        Task<ServiceResult<SessionDto>> GetSessionAsync(int userId, int sessionId);
        Task<ServiceResult<ChatResponseDto>> SendMessageAsync(int userId, int sessionId, SendMessageDto dto);
        Task<ServiceResult<List<MessageDto>>> GetMessagesAsync(int userId, int sessionId);
    }
}
