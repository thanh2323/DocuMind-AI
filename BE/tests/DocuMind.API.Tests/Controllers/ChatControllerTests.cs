using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DocuMind.API.Controllers.Chat;
using DocuMind.Application.Interface.IChat;
using DocuMind.Application.Interface.IRag;
using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.DTOs.Common;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DocuMind.API.Tests.Controllers
{
    public class ChatControllerTests
    {
        private readonly Mock<IChatService> _mockChatService;
        private readonly Mock<IRagService> _mockRagService;
        private readonly Mock<ILogger<ChatController>> _mockLogger;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _mockChatService = new Mock<IChatService>();
            _mockRagService = new Mock<IRagService>();
            _mockLogger = new Mock<ILogger<ChatController>>();
            _controller = new ChatController(_mockRagService.Object, _mockChatService.Object, _mockLogger.Object);
        }

        private void SetupUserContext(string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateChat_Authenticated_ReturnsOk()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            var dto = new CreateSessionDto { Title = "New Chat" };
            var session = new SessionDto { Id = 10, Title = "New Chat" };

            _mockChatService.Setup(s => s.CreateChatAsync(1, dto))
                .ReturnsAsync(ServiceResult<SessionDto>.Ok(session));

            // ACT
            var result = await _controller.CreateChat(dto);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<SessionDto>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(10, apiResponse.Data!.Id);
        }

        [Fact]
        public async Task CreateChat_ServiceFailure_ReturnsBadRequest()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            var dto = new CreateSessionDto { Title = "Invalid" };
            
            _mockChatService.Setup(s => s.CreateChatAsync(1, dto))
                .ReturnsAsync(ServiceResult<SessionDto>.Fail("Some error"));

            // ACT
            var result = await _controller.CreateChat(dto);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<SessionDto>>(badRequestResult.Value);
            
            Assert.False(apiResponse.Success);
            Assert.Equal("Some error", apiResponse.Message);
        }

        [Fact]
        public async Task SendMessage_Authenticated_ReturnsOk()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            var sessionId = 10;
            var dto = new SendMessageDto { Content = "Hello" };
            var response = new ChatResponseDto 
            { 
                BotMessage = new MessageDto { Content = "Hi there" } 
            };

            _mockChatService.Setup(s => s.SendMessageAsync(1, sessionId, dto))
                .ReturnsAsync(ServiceResult<ChatResponseDto>.Ok(response));

            // ACT
            var result = await _controller.SendMessage(sessionId, dto);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<ChatResponseDto>>(okResult.Value);
            
            Assert.True(apiResponse.Success);
            Assert.Equal("Hi there", apiResponse.Data!.BotMessage.Content);
        }

        [Fact]
        public async Task SendMessage_ServiceFailure_ReturnsBadRequest()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            var sessionId = 10;
            var dto = new SendMessageDto { Content = "Hello" };

            _mockChatService.Setup(s => s.SendMessageAsync(1, sessionId, dto))
                .ReturnsAsync(ServiceResult<ChatResponseDto>.Fail("Failed to send"));

            // ACT
            var result = await _controller.SendMessage(sessionId, dto);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<ChatResponseDto>>(badRequestResult.Value);
            
            Assert.False(apiResponse.Success);
            Assert.Equal("Failed to send", apiResponse.Message);
        }

        [Fact]
        public async Task GetSessions_ReturnsOk()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            var sessions = new List<SessionDto> { new SessionDto { Id = 1 }, new SessionDto { Id = 2 } };

            _mockChatService.Setup(s => s.GetSessionsAsync(1))
                .ReturnsAsync(ServiceResult<List<SessionDto>>.Ok(sessions));

            // ACT
            var result = await _controller.GetSessions();

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<List<SessionDto>>>(okResult.Value);
            
            Assert.True(apiResponse.Success);
            Assert.Equal(2, apiResponse.Data!.Count);
        }
    }
}
