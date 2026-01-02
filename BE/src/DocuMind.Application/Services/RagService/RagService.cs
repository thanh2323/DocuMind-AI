using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Rag;
using DocuMind.Application.Interface.IIntentClassifier;
using DocuMind.Application.Interface.IPrompt;
using DocuMind.Application.Interface.IRag;
using DocuMind.Core.Enum;
using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.ILLM;
using DocuMind.Core.Interfaces.IPdf;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IStorage;
using DocuMind.Core.Interfaces.IVectorDb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocuMind.Application.Services.Rag
{
    public class RagService : IRagService
    {
        private readonly IIntentClassifierService _intentClassifier;
        private readonly IPromptFactory _promptFactory; 
        private readonly IEmbeddingService _embeddingService;
        private readonly IDocumentRepository _documentRepository;
        private readonly IPdfProcessorService _pdfProcessorService;
        private readonly IStorageService _storageService;
        private readonly IChatSessionRepository _chatRepository;
        private readonly IVectorDbService _vectorDbService;
        private readonly ILlmService _llmService;
        private readonly ILogger<RagService> _logger;
        private const int TOP_K = 10;
        private const float SCORE_THRESHOLD = 0.6f;


        public RagService(
            IPromptFactory promptFactory,
            IIntentClassifierService intentClassifier,
            IDocumentRepository documentRepository,
            IPdfProcessorService pdfProcessorService,
            IStorageService storageService,
            IEmbeddingService embeddingService,
            IVectorDbService vectorDbService,
            ILlmService llmService,
            IChatSessionRepository chatRepository,
            ILogger<RagService> logger)
        {
            _promptFactory = promptFactory;
            _intentClassifier = intentClassifier;
            _documentRepository = documentRepository;
            _pdfProcessorService = pdfProcessorService;
            _storageService = storageService;
            _chatRepository = chatRepository;
            _embeddingService = embeddingService;
            _vectorDbService = vectorDbService;
            _llmService = llmService;
            _logger = logger;

            //Configurable RAG parameters
            /*   _topK = int.Parse(configuration["RagSettings:TopK"] ?? "5");
                _scoreThreshold = float.Parse(configuration["RagSettings:ScoreThreshold"] ?? "0.5");*/
        }
        public async Task<ServiceResult<RagDto>> AskQuestionAsync(string question, List<int> documentIds, int sessionId, CancellationToken cancellationToken = default)
        {
            var stopWatch = Stopwatch.StartNew();

            // Step 1: Classify Intent
            var intent = await _intentClassifier.ClassifyIntentAsync(question, cancellationToken);
            _logger.LogInformation("Processing RAG request with intent: {Intent}", intent);

            string context = "";

            // Step 2: Retrieve Context Strategy
            if (intent == IntentType.SUMMARY)
            {
                // STRATEGY: Direct File Reading (Full Context)
                _logger.LogInformation("Intent is SUMMARY. Reading full documents...");
                context = await GetFullDocumentContextAsync(documentIds, sessionId, cancellationToken);
            }
            else
            {
                // STRATEGY: Vector Search (Chunk Retrieval) for QA/EXPLANATION
                context = await GetVectorSearchContextAsync(question, documentIds, intent, cancellationToken);
            }

            // Step 3: Get conversation history
            var recentMessages = await _chatRepository.GetWithRecentMessagesAsync(sessionId, 5);
            var conversationHistory = recentMessages?.Messages
                .Select(m => $"{(m.IsUser ? "User" : "System")}: {m.Content}")
                .ToList();

            // Step 4: Create prompt using Factory
            var prompt = _promptFactory.GetPrompt(intent, question, context, conversationHistory);

            // Step 5: Generate answer
            _logger.LogDebug("Generating answer with Gemini...");
            var answer = await _llmService.AskAsync(prompt, cancellationToken);

            stopWatch.Stop();

            var returnDto = new RagDto
            {
                Answer = answer,
                ProcessingTimeMs = stopWatch.ElapsedMilliseconds
            };

            return ServiceResult<RagDto>.Ok(returnDto);
        }
        private async Task<string> GetFullDocumentContextAsync(List<int> documentIds, int sessionId, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== FULL DOCUMENT CONTENT ===");
            sb.AppendLine("The following is the full content of the documents to be summarized:");
            sb.AppendLine();

            var session = await _chatRepository.GetByIdAsync(sessionId); // Assuming standard generic repo GetByIdAsync
            int userId = session?.UserId ?? 0;

            if (userId == 0)
            {
                _logger.LogWarning("Could not determine UserId from SessionId {SessionId}. Summary might fail if Repo requires it.", sessionId);
            }

            var documents = await _documentRepository.GetDocumentsAsync(documentIds, userId);

            foreach (var doc in documents)
            {
                try
                {
                    sb.AppendLine($"--- Start of Document: {doc.FileName} ---");
                    using var stream = await _storageService.GetFileStreamAsync(doc.FilePath);
                    var text = _pdfProcessorService.ExtractText(stream, cancellationToken);
                    sb.AppendLine(text);
                    sb.AppendLine($"--- End of Document: {doc.FileName} ---");
                    sb.AppendLine();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to read document {DocId}", doc.Id);
                    sb.AppendLine($"[Error reading document {doc.FileName}]");
                }
            }

            return sb.ToString();
        }


        private async Task<string> GetVectorSearchContextAsync(string question, List<int> documentIds, IntentType intent, CancellationToken cancellationToken)
        {
            var questionEmbedding = await _embeddingService.EmbedChunkAsync(question, cancellationToken);

            // Determine search parameters based on intent
            int topK = intent switch
            {
                IntentType.EXPLANATION => 15,
                _ => 10
            };

            float scoreThreshold = intent switch
            {
                IntentType.EXPLANATION => 0.6f,
                _ => 0.65f
            };

            var searchResults = await _vectorDbService.SearchSimilarAsync(questionEmbedding, documentIds, topK);

            // Filter results
            var relevantResults = searchResults
                .Where(r => r.Score >= scoreThreshold)
                .ToList();

            if (relevantResults.Count == 0)
            {
                _logger.LogWarning("No relevant chunks found with threshold {Threshold} for intent {Intent}", scoreThreshold, intent);
                // If no results, try fallback with very low threshold for explanation
                if (intent == IntentType.EXPLANATION)
                {
                    relevantResults = searchResults.Where(r => r.Score >= 0.4f).ToList();
                }

                if (relevantResults.Count == 0)
                {
                    return "I couldn't find enough relevant information in the documents to answer your question.";
                }
            }

            return BuildContext(relevantResults);
        }

        private string BuildContext(List<SearchResult> searchResults)
        {
            var sb = new StringBuilder();

            if (searchResults == null || searchResults.Count == 0)
            {
                return string.Empty;
            }

            for (int i = 0; i < searchResults.Count; i++)
            {
                var result = searchResults[i];
                sb.AppendLine($"[Source {i + 1}] (Score: {result.Score:F2})");
                sb.AppendLine(result.ChunkText.Trim());
                sb.AppendLine();
            }

            return sb.ToString();
        }
 

    }
}
