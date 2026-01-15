using Xunit;
using Moq;
using DocuMind.Application.Services.ChatService;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Application.Interface.IChat;
using DocuMind.Application.Interface.IRag;
using DocuMind.Core.Entities;
using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Rag;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace DocuMind.Application.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatSessionRepository> _mockSessionRepo;
        private readonly Mock<IRagService> _mockRagService;
        private readonly Mock<IRepository<ChatMessage>> _mockMessageRepo;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _mockSessionRepo = new Mock<IChatSessionRepository>();
            _mockRagService = new Mock<IRagService>();
            _mockMessageRepo = new Mock<IRepository<ChatMessage>>();

            _chatService = new ChatService(
                _mockMessageRepo.Object,
                _mockRagService.Object,
                _mockSessionRepo.Object
            );
        }

        [Fact]
        public async Task CreateChat_ValidData_ReturnsSuccess()
        {
            // ARRANGE
            int userId = 1;
            var dto = new CreateSessionDto { Title = "New Chat" };

            // ACT
            var result = await _chatService.CreateChatAsync(userId, dto);

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal("New Chat", result.Data?.Title);
            
            // VERIFY
            _mockSessionRepo.Verify(repo => repo.AddAsync(It.Is<ChatSession>(s => s.Title == "New Chat" && s.UserId == userId)), Times.Once);
            _mockSessionRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SendMessage_SessionNotFound_ReturnsFailure()
        {
            // ARRANGE
            int userId = 1;
            int sessionId = 99;
            var dto = new SendMessageDto { Content = "Hello" };

            _mockSessionRepo.Setup(repo => repo.GetWithDocumentsAsync(sessionId)).ReturnsAsync((ChatSession?)null);

            // ACT
            var result = await _chatService.SendMessageAsync(userId, sessionId, dto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Contains("not found", result.Message);
        }

        [Fact]
        public async Task SendMessage_AccessDenied_ReturnsFailure()
        {
            // ARRANGE
            int userId = 1;
            int otherUserId = 2;
            int sessionId = 10;
            var session = new ChatSession { Id = sessionId, UserId = otherUserId }; // Belongs to other user

            _mockSessionRepo.Setup(repo => repo.GetWithDocumentsAsync(sessionId)).ReturnsAsync(session);

            // ACT
            var result = await _chatService.SendMessageAsync(userId, sessionId, new SendMessageDto { Content = "Hello" });

            // ASSERT
            Assert.False(result.Success);
            Assert.Contains("access denied", result.Message);
        }

        [Fact]
        public async Task SendMessage_Success_VerifyRagAndDbCalls()
        {
            // ARRANGE
            int userId = 1;
            int sessionId = 10;
            var session = new ChatSession 
            { 
                Id = sessionId, 
                UserId = userId,
                SessionDocuments = new List<SessionDocument> 
                { 
                    new SessionDocument { DocumentId = 100 } 
                } 
            };
            var dto = new SendMessageDto { Content = "What is this?" };
            
            // Mock Session Repo
            _mockSessionRepo.Setup(repo => repo.GetWithDocumentsAsync(sessionId)).ReturnsAsync(session);

            // Mock RAG Service (The important part!)
            var ragResponse = new RagDto { Answer = "This is a document." };
            _mockRagService.Setup(r => r.AskQuestionAsync(dto.Content, It.IsAny<List<int>>(), sessionId, CancellationToken.None))
                .ReturnsAsync(ServiceResult<RagDto>.Ok(ragResponse));

            // ACT
            var result = await _chatService.SendMessageAsync(userId, sessionId, dto);

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal("This is a document.", result.Data?.BotMessage.Content);

            // VERIFY INTERACTIONS
            // 1. Check if user message was saved
            _mockMessageRepo.Verify(repo => repo.AddAsync(It.Is<ChatMessage>(m => m.IsUser == true && m.Content == "What is this?")), Times.Once);
            
            // 2. Check if bot message was saved
            _mockMessageRepo.Verify(repo => repo.AddAsync(It.Is<ChatMessage>(m => m.IsUser == false && m.Content == "This is a document.")), Times.Once);
            
            // 3. Check if changes were saved (called 2 times, once for each message)
            _mockMessageRepo.Verify(repo => repo.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task SendMessage_RagFailure_ReturnsFailure()
        {
            // ARRANGE
            int userId = 1;
            int sessionId = 10;
            var session = new ChatSession 
            { 
                Id = sessionId, 
                UserId = userId,
                SessionDocuments = new List<SessionDocument> { new SessionDocument { DocumentId = 100 } } 
            };
            
            _mockSessionRepo.Setup(repo => repo.GetWithDocumentsAsync(sessionId)).ReturnsAsync(session);

            // Mock RAG Failure
            _mockRagService.Setup(r => r.AskQuestionAsync(It.IsAny<string>(), It.IsAny<List<int>>(), sessionId, CancellationToken.None))
                .ReturnsAsync(ServiceResult<RagDto>.Fail("AI Service Unavailable"));

            // ACT
            var result = await _chatService.SendMessageAsync(userId, sessionId, new SendMessageDto { Content = "Hi" });

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("AI Service Unavailable", result.Message);
        }
    }
}
