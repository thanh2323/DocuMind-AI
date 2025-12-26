using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.Interface.IChat;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Application.Interface.IRag;
using DocuMind.Application.Services.DocumentService;
using DocuMind.Core.Entities;
using DocuMind.Core.Interfaces.IRepo;

namespace DocuMind.Application.Services.ChatService
{
    public class ChatService : IChatService
    {
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly ISessionDocumentRepository _sessionDocumentRepository;
        private readonly IRagService _ragService;
        private readonly IRepository<ChatMessage> _chatMessage;
        private readonly IDocumentService _documentService;

        public ChatService(IRepository<ChatMessage> chatMessage, IRagService ragService, ISessionDocumentRepository sessionDocumentRepository, IDocumentService documentService, IChatSessionRepository chatSessionRepository)
        {
            _chatMessage = chatMessage;
            _ragService = ragService;
            _sessionDocumentRepository = sessionDocumentRepository;
            _documentService = documentService;
            _chatSessionRepository = chatSessionRepository;
        }
        /*   public async Task<ServiceResult<SessionDto>> CreateChatAsync(int userId, CreateSessionDto dto)
           {
               var documents = await _documentService.GetByIdsAsync(userId, dto.DocumentIds);

               if (documents == null)
                   return ServiceResult<SessionDto>.Fail("One or more documents are not processed yet or encountered an error");

               var session = new ChatSession
               {
                   UserId = userId,
                   Title = dto.Title,
                   CreatedAt = DateTime.UtcNow,
                   LastActiveAt = DateTime.UtcNow
               };
               await _chatSessionRepository.AddAsync(session);
               await _chatSessionRepository.SaveChangesAsync();

               var sessionDocuments = dto.DocumentIds.Select(docId => new SessionDocument
               {
                   SessionId = session.Id,
                   DocumentId = docId,
                   AddedAt = DateTime.UtcNow
               }).ToList();

               await _sessionDocumentRepository.AddRangeAsync(sessionDocuments);
               await _sessionDocumentRepository.SaveChangesAsync();

               var sessionDto = new SessionDto
               {
                   Id = session.Id,
                   Title = session.Title,
                   CreatedAt = session.CreatedAt,
                   LastActiveAt = session.LastActiveAt,
                   MessageCount = 0,
                   Documents = documents.Data!
               };
               return ServiceResult<SessionDto>.Ok(sessionDto);


           }*/

        public async Task<ServiceResult<SessionDto>> CreateChatAsync(int userId, CreateSessionDto dto)
        {
            var session = new ChatSession
            {
                UserId = userId,
                Title = dto.Title,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow
            };

            await _chatSessionRepository.AddAsync(session);
            await _chatSessionRepository.SaveChangesAsync();


            return ServiceResult<SessionDto>.Ok(new SessionDto
            {
                Id = session.Id,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                LastActiveAt = session.LastActiveAt,
                MessageCount = 0,
                Documents = []
            });
        }

        public async Task<ServiceResult<ChatResponseDto>> SendMessageAsync(int userId, int sessionId, SendMessageDto dto)
        {
            var session = await _chatSessionRepository.GetWithDocumentsAsync(sessionId);
            if (session == null || session.UserId != userId)
            {
                return ServiceResult<ChatResponseDto>.Fail("Chat session not found or access denied.");
            }


            var documentIds = session.SessionDocuments
                .Select(sd => sd.DocumentId)
                .ToList();

            if (!documentIds.Any())
                return ServiceResult<ChatResponseDto>.Fail("No documents in this session");


            var userMessage = new ChatMessage
            {
                SessionId = sessionId,
                Content = dto.Content,
                IsUser = true,
                Timestamp = DateTime.UtcNow
            };

            await _chatMessage.AddAsync(userMessage);
            await _chatMessage.SaveChangesAsync();

            var ragResult = await _ragService.AskQuestionAsync(dto.Content, documentIds, sessionId);

            if (!ragResult.Success)
            {
                return ServiceResult<ChatResponseDto>.Fail(ragResult.Message);
            }
            // 4. Save bot message
            var botMessage = new ChatMessage
            {
                SessionId = sessionId,
                Content = ragResult.Data!.Answer,
                IsUser = false,
                Timestamp = DateTime.UtcNow
            };

            await _chatMessage.AddAsync(botMessage);

            //// 5. Update session activity
            //session.LastActiveAt = DateTime.UtcNow;
            //await _chatSessionRepository.AddAsync(session);

            //await _chatSessionRepository.SaveChangesAsync();

            // 6. Build response DTO
            var response = new ChatResponseDto
            {
                UserMessage = new MessageDto
                {
                    Id = userMessage.Id,
                    Content = userMessage.Content,
                    IsUser = true,
                    Timestamp = userMessage.Timestamp
                },
                BotMessage = new MessageDto
                {
                    Id = botMessage.Id,
                    Content = botMessage.Content,
                    IsUser = false,
                    Timestamp = botMessage.Timestamp
                },
                ProcessingTimeMs = ragResult.Data!.ProcessingTimeMs
            };

            return ServiceResult<ChatResponseDto>.Ok(response);
        }
    }
}

