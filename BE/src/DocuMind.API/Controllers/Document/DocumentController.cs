using DocuMind.Application.DTOs.Common;
using System.Security.Claims;
using DocuMind.Application.DTOs.Document;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Application.Services.UserService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Google.GenAI;

namespace DocuMind.API.Controllers.Document
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }
        [HttpPost("sessions/{sessionId}/upload")]
        public async Task<IActionResult> UploadDocument(int sessionId, [FromForm] UploadDocumentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _documentService.UploadDocument(int.Parse(userId!), sessionId, dto);

            if (!result.Success)
                return BadRequest(ApiResponse<DocumentItemDto>.ErrorResponse(result.Message));

            return Ok(result.Data);
        }

        [HttpPost("status")]
        public async Task<IActionResult> CheckStatus([FromBody] List<int> documentIds)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _documentService.CheckStatusAsync(int.Parse(userId), documentIds);

            if (!result.Success)
                return BadRequest(ApiResponse<List<DocumentItemDto>>.ErrorResponse(result.Message));

            return Ok(result.Data);
        }
    }
}
