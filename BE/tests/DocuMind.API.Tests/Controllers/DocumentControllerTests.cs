using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using DocuMind.API.Controllers.Document;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Application.DTOs.Document;
using DocuMind.Application.DTOs.Common;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DocuMind.API.Tests.Controllers
{
    public class DocumentControllerTests
    {
        private readonly Mock<IDocumentService> _mockDocumentService;
        private readonly DocumentController _controller;

        public DocumentControllerTests()
        {
            _mockDocumentService = new Mock<IDocumentService>();
            _controller = new DocumentController(_mockDocumentService.Object);
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
        public async Task UploadDocument_Authenticated_ReturnsOk()
        {
            // ARRANGE
            var userId = "1";
            var sessionId = 10;
            SetupUserContext(userId);
            
            var fileMock = new Mock<IFormFile>();
            var dto = new UploadDocumentDto { File = fileMock.Object };
            var responseDto = new DocumentItemDto { Id = 100, FileName = "test.pdf" };

            _mockDocumentService.Setup(s => s.UploadDocument(1, sessionId, dto))
                .ReturnsAsync(ServiceResult<DocumentItemDto>.Ok(responseDto));

            // ACT
            var result = await _controller.UploadDocument(sessionId, dto);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDto = Assert.IsType<DocumentItemDto>(okResult.Value);
            
            Assert.Equal(100, returnedDto.Id);
            Assert.Equal("test.pdf", returnedDto.FileName);
        }

        [Fact]
        public async Task UploadDocument_ServiceFailure_ReturnsBadRequest()
        {
            // ARRANGE
            var userId = "1";
            var sessionId = 10;
            SetupUserContext(userId);
            
            var dto = new UploadDocumentDto { File = null! }; // Failure case

            _mockDocumentService.Setup(s => s.UploadDocument(1, sessionId, dto))
                .ReturnsAsync(ServiceResult<DocumentItemDto>.Fail("Upload failed"));

            // ACT
            var result = await _controller.UploadDocument(sessionId, dto);

            // ASSERT
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<DocumentItemDto>>(badRequestResult.Value);
            
            Assert.False(apiResponse.Success);
            Assert.Equal("Upload failed", apiResponse.Message);
        }

        [Fact]
        public async Task CheckStatus_ReturnsOk()
        {
            // ARRANGE
            var userId = "1";
            SetupUserContext(userId);
            var docIds = new List<int> { 1, 2 };
            var responseList = new List<DocumentItemDto> 
            { 
                new DocumentItemDto { Id = 1 }, 
                new DocumentItemDto { Id = 2 } 
            };

            _mockDocumentService.Setup(s => s.CheckStatusAsync(1, docIds))
                .ReturnsAsync(ServiceResult<List<DocumentItemDto>>.Ok(responseList));

            // ACT
            var result = await _controller.CheckStatus(docIds);

            // ASSERT
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsType<List<DocumentItemDto>>(okResult.Value);
            
            Assert.Equal(2, returnedList.Count);
        }
    }
}
