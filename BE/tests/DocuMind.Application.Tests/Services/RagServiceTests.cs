using Xunit;
using Moq;
using DocuMind.Application.Services.RagService;
using DocuMind.Application.Interface.IIntentClassifier;
using DocuMind.Application.Interface.IPrompt;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IPdf;
using DocuMind.Core.Interfaces.IStorage;
using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.IVectorDb;
using DocuMind.Core.Interfaces.ILLM;
using DocuMind.Core.Entities;
using DocuMind.Application.DTOs.Common;
using DocuMind.Core.Enum;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System;

namespace DocuMind.Application.Tests.Services
{
    public class RagServiceTests
    {
        private readonly Mock<IPromptFactory> _mockPromptFactory;
        private readonly Mock<IIntentClassifierService> _mockClassifier;
        private readonly Mock<IDocumentRepository> _mockDocRepo;
        private readonly Mock<IPdfProcessorService> _mockPdfService;
        private readonly Mock<IStorageService> _mockStorage;
        private readonly Mock<IEmbeddingService> _mockEmbedding;
        private readonly Mock<IVectorDbService> _mockVectorDb;
        private readonly Mock<ILlmService> _mockLlm;
        private readonly Mock<IChatSessionRepository> _mockChatRepo;
        private readonly Mock<ILogger<RagService>> _mockLogger;
        private readonly RagService _ragService;

        public RagServiceTests()
        {
            _mockPromptFactory = new Mock<IPromptFactory>();
            _mockClassifier = new Mock<IIntentClassifierService>();
            _mockDocRepo = new Mock<IDocumentRepository>();
            _mockPdfService = new Mock<IPdfProcessorService>();
            _mockStorage = new Mock<IStorageService>();
            _mockEmbedding = new Mock<IEmbeddingService>();
            _mockVectorDb = new Mock<IVectorDbService>();
            _mockLlm = new Mock<ILlmService>();
            _mockChatRepo = new Mock<IChatSessionRepository>();
            _mockLogger = new Mock<ILogger<RagService>>();

            _ragService = new RagService(
                _mockPromptFactory.Object,
                _mockClassifier.Object,
                _mockDocRepo.Object,
                _mockPdfService.Object,
                _mockStorage.Object,
                _mockEmbedding.Object,
                _mockVectorDb.Object,
                _mockLlm.Object,
                _mockChatRepo.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task AskQuestion_SummaryIntent_UsesDirectFileReading()
        {
            // ARRANGE
            int sessionId = 10;
            var question = "Summarize this";
            var docIds = new List<int> { 1 };
            
            // 1. Mock Intent -> SUMMARY
            _mockClassifier.Setup(c => c.ClassifyIntentAsync(question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(IntentType.SUMMARY);

            // 2. Mock Chat Session (to get UserId)
            _mockChatRepo.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(new ChatSession { Id = 10, UserId = 1 });

            // 3. Mock Document Retrieval
            var docs = new List<Document> { new Document { Id = 1, FilePath = "path/to/file.pdf", FileName = "file.pdf" } };
            _mockDocRepo.Setup(r => r.GetDocumentsAsync(docIds, 1)).ReturnsAsync(docs);

            // 4. Mock Storage & PDF Processing (Direct Read Strategy)
            _mockStorage.Setup(s => s.GetFileStreamAsync("path/to/file.pdf")).ReturnsAsync(new MemoryStream());
            _mockPdfService.Setup(p => p.ExtractText(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns("Extracted Text Content");

            // 5. Mock Prompt Factory
            _mockPromptFactory.Setup(p => p.GetPrompt(IntentType.SUMMARY, question, It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("Final Prompt");

            // 6. Mock LLM Success
            _mockLlm.Setup(l => l.AskAsync("Final Prompt", It.IsAny<CancellationToken>())).ReturnsAsync("Summary Answer");

            // ACT
            var result = await _ragService.AskQuestionAsync(question, docIds, sessionId);

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal("Summary Answer", result.Data.Answer);

            // VERIFY Strategy Execution
            _mockPdfService.Verify(p => p.ExtractText(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once); // Should read file
            _mockEmbedding.Verify(e => e.EmbedChunkAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never); // Should NOT embed
            _mockVectorDb.Verify(v => v.SearchSimilarAsync(It.IsAny<float[]>(), It.IsAny<List<int>>(), It.IsAny<int>()), Times.Never); // Should NOT vector search
        }

        [Fact]
        public async Task AskQuestion_QaIntent_UsesVectorSearch()
        {
            // ARRANGE
            int sessionId = 10;
            var question = "What is X?";
            var docIds = new List<int> { 1 };

            // 1. Mock Intent -> QA
            _mockClassifier.Setup(c => c.ClassifyIntentAsync(question, It.IsAny<CancellationToken>()))
                .ReturnsAsync(IntentType.QA);

            // 2. Mock Embedding
            var fakeEmbedding = new float[] { 0.1f, 0.2f };
            _mockEmbedding.Setup(e => e.EmbedChunkAsync(question, It.IsAny<CancellationToken>())).ReturnsAsync(fakeEmbedding);

            // 3. Mock Vector DB Search
            var searchResults = new List<SearchResult> 
            { 
                new SearchResult { Score = 0.8f, ChunkText = "Relevant Chunk" } 
            };
            _mockVectorDb.Setup(v => v.SearchSimilarAsync(fakeEmbedding, docIds, It.IsAny<int>())).ReturnsAsync(searchResults);

            // 4. Mock Prompt Factory
            _mockPromptFactory.Setup(p => p.GetPrompt(IntentType.QA, question, It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("Final QA Prompt");

            // 5. Mock LLM
            _mockLlm.Setup(l => l.AskAsync("Final QA Prompt", It.IsAny<CancellationToken>())).ReturnsAsync("QA Answer");

            // ACT
            var result = await _ragService.AskQuestionAsync(question, docIds, sessionId);

            // ASSERT
            Assert.True(result.Success);
            Assert.Equal("QA Answer", result.Data.Answer);

            // VERIFY Strategy Execution
            _mockVectorDb.Verify(v => v.SearchSimilarAsync(fakeEmbedding, docIds, It.IsAny<int>()), Times.Once); // Should vector search
            _mockPdfService.Verify(p => p.ExtractText(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never); // Should NOT read full file directly
        }

        [Fact]
        public async Task AskQuestion_LlmFailure_ReturnsFailure()
        {
            // ARRANGE
            var question = "Fail me";
            _mockClassifier.Setup(c => c.ClassifyIntentAsync(question, It.IsAny<CancellationToken>())).ReturnsAsync(IntentType.QA);
            _mockEmbedding.Setup(e => e.EmbedChunkAsync(question, It.IsAny<CancellationToken>())).ReturnsAsync(new float[0]);
             _mockVectorDb.Setup(v => v.SearchSimilarAsync(It.IsAny<float[]>(), It.IsAny<List<int>>(), It.IsAny<int>()))
                .ReturnsAsync(new List<SearchResult> { new SearchResult { Score = 0.9f, ChunkText = "C" } });
            
            _mockPromptFactory.Setup(p => p.GetPrompt(It.IsAny<IntentType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns("Prompt");

            // Mock Failure
            _mockLlm.Setup(l => l.AskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("API Error"));

            // ACT
            var result = await _ragService.AskQuestionAsync(question, new List<int>(), 1);

            // ASSERT
            Assert.False(result.Success);
            Assert.Contains("Failed", result.Message);
        }
    }
}
