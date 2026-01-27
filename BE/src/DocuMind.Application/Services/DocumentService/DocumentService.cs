using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Document;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Application.Options;
using DocuMind.Core.Entities;
using DocuMind.Core.Enum;
using DocuMind.Core.Interfaces.IBackgroundJob;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IStorage;
using DocuMind.Core.Interfaces.IVectorDb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocuMind.Application.Services.DocumentService
{
    public class DocumentService :  IDocumentService
    {
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IChatSessionRepository _chatSessionRepository;
        private readonly ISessionDocumentRepository _sessionDocumentRepository;
        private readonly IVectorDbService _vectorDbService;
        private readonly FileUploadOptions _options;
        private readonly IStorageService _storageService;
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentService> _logger;
        public DocumentService(
            ISessionDocumentRepository sessionDocumentRepository,
            IChatSessionRepository chatSessionRepository,
            IStorageService storageService,
            IBackgroundJobService backgroundJobService,
            IDocumentRepository documentRepository,
            IOptions<FileUploadOptions> options, 
            IVectorDbService vectorDbService,
            ILogger<DocumentService> logger)
        {
            _chatSessionRepository = chatSessionRepository;
            _sessionDocumentRepository = sessionDocumentRepository;
            _documentRepository = documentRepository;
            _storageService = storageService;
            _backgroundJobService = backgroundJobService;
            _vectorDbService = vectorDbService;
            _options = options.Value;
            _logger = logger;
        }
        public async Task<ServiceResult<bool>> DeleteAsync(int userId, int documentId, bool isAdmin)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
            {
                return ServiceResult<bool>.Fail("Document not found");
            }

            if (document.UserId != userId && !isAdmin)
            {
                return ServiceResult<bool>.Fail("Access denied");
            }

            try
            {
                // 1. Delete from Storage
                if (!string.IsNullOrEmpty(document.FilePath))
                {
                    await _storageService.DeleteAsync(document.FilePath);
                }

                // 2. Delete from Vector DB
                await _vectorDbService.DeleteDocumentVectorsAsync(document.Id);

                // 3. Delete from SessionDocuments (Join Table)
                await _sessionDocumentRepository.DeleteByDocumentIdAsync(document.Id);

                // 4. Delete from Database
                await _documentRepository.DeleteAsync(document);
                await _documentRepository.SaveChangesAsync();

                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
                return ServiceResult<bool>.Fail("Failed to delete document");
            }
        }

        public async Task<ServiceResult<List<DocumentItemDto>>> CheckStatusAsync(int userId, List<int> documentIds)
        {
            var documents = await _documentRepository.GetDocumentsAsync(documentIds, userId);

            var result = documents.Select(d => new DocumentItemDto
            {
                Id = d.Id,
                FileName = d.FileName,
                FileSize = d.FileSize,
                Status = d.Status,
                CreatedAt = d.CreatedAt
            }).ToList();

            return ServiceResult<List<DocumentItemDto>>.Ok(result);
        }

        public async Task<ServiceResult<List<DocumentItemDto>>> GetByIdsAsync(int userId, List<int> documentIds)
        {
            var documents = await _documentRepository.GetDocumentsAsync(documentIds, userId);

            var errorDocument = documents.Any(d => d.Status == DocumentStatus.Error || d.Status == DocumentStatus.Pending);
            if (errorDocument)
            {
                return ServiceResult<List<DocumentItemDto>>.Fail("Some documents are not ready yet");
            }

            var result = documents.Select(d => new DocumentItemDto
           {
               Id = d.Id,
               FileName = d.FileName,
               FileSize = d.FileSize,
               Status = d.Status,
               CreatedAt = d.CreatedAt
           }).ToList();

            return ServiceResult<List<DocumentItemDto>>.Ok(result);

        }

        public async Task<ServiceResult<DocumentItemDto>> UploadDocument(int userId, int sessionId, UploadDocumentDto dto)
        {
            var file = dto.File;

            if (file == null || file.Length == 0)
                return ServiceResult<DocumentItemDto>.Fail("No file uploaded");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_options.AllowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type");

            if (file.Length > _options.MaxFileSizeMB * 1024 * 1024)
                throw new ArgumentException($"File exceeds {_options.MaxFileSizeMB}MB");


            var session = await _chatSessionRepository.GetByIdAsync(sessionId);
            if (session == null || session.UserId != userId)
                return ServiceResult<DocumentItemDto>.Fail("Invalid chat session");


            var supabasePath = await _storageService.UploadAsync(file.OpenReadStream(), file.FileName, userId);

            var document = new Document
            {
                UserId = userId,
                FileName = file.FileName,
                FileSize = file.Length,
                FilePath = supabasePath,
                Status = DocumentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            var sessionDocument = new SessionDocument
            {
                SessionId = sessionId,
                DocumentId = document.Id,
                AddedAt = DateTime.UtcNow
            };

            await _sessionDocumentRepository.AddAsync(sessionDocument);
            await _sessionDocumentRepository.SaveChangesAsync();

            _backgroundJobService.EnqueueDocumentProcessing(document.Id);

            var returnDto = new DocumentItemDto
            {
                Id = document.Id,
                FileName = document.FileName,
                FileSize = document.FileSize,
                Status = document.Status,
                CreatedAt = document.CreatedAt
            };

            return ServiceResult<DocumentItemDto>.Ok(returnDto);
        }

        public async Task<ServiceResult<(Stream Stream, string ContentType, string FileName)>> GetDocumentContent(int userId, int documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);

            if (document == null)
            {
                return ServiceResult<(Stream Stream, string ContentType, string FileName)>.Fail("Document not found");
            }

            if (document.UserId != userId)
            {
                return ServiceResult<(Stream Stream, string ContentType, string FileName)>.Fail("Access denied");
            }

            try
            {
                var stream = await _storageService.GetFileStreamAsync(document.FilePath);
                var contentType = GetContentType(document.FileName);
                return ServiceResult<(Stream Stream, string ContentType, string FileName)>.Ok((stream, contentType, document.FileName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document content for document {DocumentId}", documentId);
                return ServiceResult<(Stream Stream, string ContentType, string FileName)>.Fail("Failed to retrieve document content");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}
