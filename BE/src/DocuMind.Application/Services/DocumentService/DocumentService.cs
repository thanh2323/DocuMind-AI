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
        public Task<ServiceResult<bool>> DeleteAsync(int userId, int documentId, bool isAdmin)
        {
            throw new NotImplementedException();
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
    }
}
