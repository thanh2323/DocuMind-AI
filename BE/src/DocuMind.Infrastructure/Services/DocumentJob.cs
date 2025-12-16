using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;
using DocuMind.Core.Enum;
using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.IPdf;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IVectorDb;
using Microsoft.Extensions.Logging;

namespace DocuMind.Infrastructure.Services
{
    public class DocumentJob
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IPdfProcessorService _pdfProcessor;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorDbService _vectorDb;
        private readonly ILogger<DocumentJob> _logger;


        public DocumentJob(IDocumentRepository documentRepository ,IPdfProcessorService pdfProcessor, IEmbeddingService embeddingService, IVectorDbService vectorDb, ILogger<DocumentJob> logger)
        {
            _documentRepository = documentRepository;
            _pdfProcessor = pdfProcessor;
            _embeddingService = embeddingService;
            _vectorDb = vectorDb;
            _logger = logger;
        }

        public async Task ProcessDocumentAsync(int documentId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("🚀 Starting document processing for DocumentId: {DocumentId}", documentId);

            var startTime = DateTime.UtcNow;

            try
            {
                // Step 1: Get document from database
                var document = await _documentRepository.GetByIdAsync(documentId);

                if (document == null)
                {
                    _logger.LogError("❌ Document not found: {DocumentId}", documentId);
                    return;
                }

                if (document.Status != DocumentStatus.Pending)
                {
                    _logger.LogWarning("⚠️ Document already processed: {DocumentId}", documentId);
                    return;
                }

                _logger.LogInformation("📄 Processing document: {FileName}", document.FileName);

                // Step 2: Validate PDF
                _logger.LogInformation("🔍 Validating PDF...");
                var isValid = _pdfProcessor.ValidatePdf(document.FilePath);
                if (!isValid)
                {
                    await MarkAsError(document, "Invalid or corrupted PDF file");
                    return;
                }

                // Step 3: Extract and clean text from PDF
                _logger.LogInformation("📖 Extracting and cleaning text from PDF...");
                var cleanText = _pdfProcessor.ExtractCleanText(document.FilePath);
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(cleanText))
                {
                    await MarkAsError(document, "No text could be extracted from PDF");
                    return;
                }

                _logger.LogInformation("✅ Extracted {Length} characters of clean text", cleanText.Length);

                // Step 4: hybrid Semantic chunking (paragraph + sentence + Structure)
                _logger.LogInformation("✂️ Performing semantic chunking...");
                var chunks = _pdfProcessor.ChunkSemantic(cleanText, chunkSize: 500, overlap: 50);

                if (chunks.Count == 0)
                {
                    await MarkAsError(document, "Failed to chunk document text");
                    return;
                }

                _logger.LogInformation("✅ Created {ChunkCount} semantic chunks", chunks.Count);

                // Step 5: Generate embeddings for all chunks
                _logger.LogInformation("🧠 Generating embeddings for {Count} chunks...", chunks.Count);
                var vectors = await _embeddingService.EmbedChunksAsync(chunks, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();


                if (vectors.Count != chunks.Count)
                {
                    await MarkAsError(document, $"Embedding count mismatch: {vectors.Count} vs {chunks.Count}");
                    return;
                }

                _logger.LogInformation("✅ Generated {Count} embeddings", vectors.Count);

                // Step 6: Store vectors in Qdrant
                _logger.LogInformation("💾 Storing vectors in Qdrant...");
              await _vectorDb.UpsertVectorsAsync(documentId, chunks, vectors);
                _logger.LogInformation("✅ Vectors stored successfully");

               /* // Step 7: Generate summary using LLM
                _logger.LogInformation("📝 Generating document summary...");
                var summary = await GenerateSummaryAsync(cleanText, cancellationToken);
                document.Summary = summary;
                _logger.LogInformation("✅ Summary generated");
*/
                // Step 8: Mark document as ready
                document.Status = DocumentStatus.Ready;
                document.ProcessedAt = DateTime.UtcNow;
                await _documentRepository.UpdateAsync(document);
                await _documentRepository.SaveChangesAsync();

                var processingTime = (DateTime.UtcNow - startTime).TotalSeconds;
                _logger.LogInformation(
                    "✅ Document processing completed successfully in {Time:F2}s - DocumentId: {DocumentId}",
                    processingTime,
                    documentId
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("⚠️ Document processing cancelled: {DocumentId}", documentId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing document: {DocumentId}", documentId);

                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document != null)
                {
                    await MarkAsError(document, $"Processing error: {ex.Message}");
                }
            }
        }

        public async Task CleanupFailedDocumentsAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("🧹 Starting cleanup of failed documents...");

                var cutoffDate = DateTime.UtcNow.AddDays(-7);
                var failedDocuments = await _documentRepository.GetByStatusAsync(DocumentStatus.Error);
                var documentsToDelete = failedDocuments.ToList();

                if (documentsToDelete.Count == 0)
                {
                    _logger.LogInformation("No failed documents to cleanup");
                    return;
                }

                _logger.LogInformation("Found {Count} failed documents to cleanup",
                    documentsToDelete.Count);

                foreach (var doc in documentsToDelete)
                {
                    try
                    {
                        // Delete file from disk
                        if (File.Exists(doc.FilePath))
                        {
                            File.Delete(doc.FilePath);
                        }

                        // Delete from database
                        await _documentRepository.DeleteAsync(doc);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error cleaning up document: {DocumentId}", doc.Id);
                    }
                }

                await _documentRepository.SaveChangesAsync();
                _logger.LogInformation("✅ Cleanup completed. Removed {Count} documents",
                    documentsToDelete.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup job");
            }
        }

        private async Task MarkAsError(Document document, string errorMessage)
        {
            _logger.LogError("Marking document as error: {DocumentId} - {Error}",
                document.Id, errorMessage);

            document.Status = DocumentStatus.Error;
            document.Summary = $"Error: {errorMessage}";
            await _documentRepository.UpdateAsync(document);
            await _documentRepository.SaveChangesAsync();
        }


    }
}