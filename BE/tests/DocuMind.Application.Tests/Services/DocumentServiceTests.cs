using Xunit;
using Moq;
using DocuMind.Application.Services.DocumentService;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IStorage;
using DocuMind.Core.Interfaces.IBackgroundJob;
using DocuMind.Core.Interfaces.IVectorDb;
using DocuMind.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocuMind.Application.DTOs.Document;
using DocuMind.Core.Entities;
using DocuMind.Core.Enum;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace DocuMind.Application.Tests.Services
{
    public class DocumentServiceTests
    {
        private readonly Mock<ISessionDocumentRepository> _mockSessionDocRepo;
        private readonly Mock<IChatSessionRepository> _mockChatRepo;
        private readonly Mock<IStorageService> _mockStorageService;
        private readonly Mock<IBackgroundJobService> _mockJobService;
        private readonly Mock<IDocumentRepository> _mockDocRepo;
        private readonly Mock<IOptions<FileUploadOptions>> _mockOptions;
        private readonly Mock<IVectorDbService> _mockVectorDbService;
        private readonly Mock<ILogger<DocumentService>> _mockLogger;
        private readonly DocumentService _documentService;

        public DocumentServiceTests()
        {
            _mockSessionDocRepo = new Mock<ISessionDocumentRepository>();
            _mockChatRepo = new Mock<IChatSessionRepository>();
            _mockStorageService = new Mock<IStorageService>();
            _mockJobService = new Mock<IBackgroundJobService>();
            _mockDocRepo = new Mock<IDocumentRepository>();
            _mockOptions = new Mock<IOptions<FileUploadOptions>>();
            _mockVectorDbService = new Mock<IVectorDbService>();
            _mockLogger = new Mock<ILogger<DocumentService>>();

            // Default Options
            _mockOptions.Setup(o => o.Value).Returns(new FileUploadOptions
            {
                AllowedExtensions = new[] { ".pdf" },
                MaxFileSizeMB = 10
            });

            _documentService = new DocumentService(
                _mockSessionDocRepo.Object,
                _mockChatRepo.Object,
                _mockStorageService.Object,
                _mockJobService.Object,
                _mockDocRepo.Object,
                _mockOptions.Object,
                _mockVectorDbService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task UploadDocument_ValidFile_ReturnsSuccess()
        {
            // ARRANGE
            int userId = 1;
            int sessionId = 10;
            var fileMock = new Mock<IFormFile>();
            var content = "Fake PDF Content";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            var dto = new UploadDocumentDto { File = fileMock.Object };

            // Mock Session
            var session = new ChatSession { Id = sessionId, UserId = userId };
            _mockChatRepo.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(session);

            // Mock Storage
            _mockStorageService.Setup(s => s.UploadAsync(It.IsAny<Stream>(), fileName, userId)).ReturnsAsync("path/to/supabase/test.pdf");

            // ACT
            var result = await _documentService.UploadDocument(userId, sessionId, dto);

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal(fileName, result.Data.FileName);
            
            // VERIFY
            _mockStorageService.Verify(s => s.UploadAsync(It.IsAny<Stream>(), fileName, userId), Times.Once); // Storage Called
            _mockDocRepo.Verify(r => r.AddAsync(It.Is<Document>(d => d.FileName == fileName && d.Status == DocumentStatus.Pending)), Times.Once); // DB Document Saved
            _mockSessionDocRepo.Verify(r => r.AddAsync(It.Is<SessionDocument>(sd => sd.SessionId == sessionId)), Times.Once); // DB SessionDoc Saved
            _mockJobService.Verify(j => j.EnqueueDocumentProcessing(It.IsAny<int>()), Times.Once); // Background Job Enqueued
        }

        [Fact]
        public async Task UploadDocument_NoFile_ReturnsFailure()
        {
            // ARRANGE
            int userId = 1;
            int sessionId = 10;
            // Case 1: Null File
            var dto = new UploadDocumentDto { File = null };

            // ACT
            var result = await _documentService.UploadDocument(userId, sessionId, dto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("No file uploaded", result.Message);
        }

        [Fact]
        public async Task UploadDocument_InvalidExtension_ThrowsException()
        {
            // NOTE: The service originally threw ArgumentException for invalid files, 
            // but we might want to catch it or assert it throws.
            
            // ARRANGE
            int userId = 1;
            int sessionId = 10;
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.exe"); // .exe not allowed
            fileMock.Setup(f => f.Length).Returns(100);
            var dto = new UploadDocumentDto { File = fileMock.Object };

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await _documentService.UploadDocument(userId, sessionId, dto));
        }

        [Fact]
        public async Task UploadDocument_FileTooLarge_ThrowsException()
        {
            // ARRANGE
            int userId = 1;
            int sessionId = 10;
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.Length).Returns(20 * 1024 * 1024); // 20MB > 10MB limit set in constructor
            var dto = new UploadDocumentDto { File = fileMock.Object };

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await _documentService.UploadDocument(userId, sessionId, dto));
        }

        [Fact]
        public async Task UploadDocument_InvalidSession_ReturnsFailure()
        {
             // ARRANGE
            int userId = 1;
            int sessionId = 99;
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.Length).Returns(100);
            var dto = new UploadDocumentDto { File = fileMock.Object };

            // Mock Session Not Found
            _mockChatRepo.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync((ChatSession)null);

            // ACT
            var result = await _documentService.UploadDocument(userId, sessionId, dto);

            // ASSERT
            Assert.False(result.Success);
            Assert.Equal("Invalid chat session", result.Message);
        }

        [Fact]
        public async Task GetByIdsAsync_Success()
        {
            // ARRANGE
            int userId = 1;
            var docIds = new List<int> { 1, 2 };
            var docs = new List<Document>
            {
                new Document { Id = 1, FileName = "doc1.pdf", Status = DocumentStatus.Ready },
                new Document { Id = 2, FileName = "doc2.pdf", Status = DocumentStatus.Ready }
            };

            _mockDocRepo.Setup(r => r.GetDocumentsAsync(docIds, userId)).ReturnsAsync(docs);

            // ACT
            var result = await _documentService.GetByIdsAsync(userId, docIds);

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetByIdsAsync_DocumentsNotReady_ReturnsFailure()
        {
            // ARRANGE
            int userId = 1;
            var docIds = new List<int> { 1 };
            var docs = new List<Document>
            {
                new Document { Id = 1, FileName = "doc1.pdf", Status = DocumentStatus.Pending } // PENDING
            };

            _mockDocRepo.Setup(r => r.GetDocumentsAsync(docIds, userId)).ReturnsAsync(docs);

            // ACT
            var result = await _documentService.GetByIdsAsync(userId, docIds);

            // ASSERT
            Assert.False(result.Success);
            Assert.Contains("not ready", result.Message);
        }
    }
}
