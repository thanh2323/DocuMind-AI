using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.IVectorDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuMind.API.Controllers.VectorDb
{
    [ApiController]
    [Authorize(Policy = "Admin")]
    [Route("api/[controller]")]
    public class VectorDbController : ControllerBase
    {
        private readonly IVectorDbService _vectorDbService;
        private readonly IEmbeddingService _embeddingService;

        public VectorDbController(IVectorDbService vectorDbService, IEmbeddingService embeddingService)
        {
            _vectorDbService = vectorDbService;
            _embeddingService = embeddingService;
        }

        [HttpPost("init")]
        public async Task<IActionResult> Initialize()
        {
            await _vectorDbService.InitializeCollectionAsync();
            return Ok(new { message = "Vector DB initialized successfully" });
        }

     /*   [HttpGet("exists")]
        public async Task<IActionResult> CheckExists()
        {
            var exists = await _vectorDbService.CollectionExistsAsync();
            return Ok(new { exists });
        }

        [HttpDelete("documents/{documentId}")]
        public async Task<IActionResult> DeleteDocumentVectors(int documentId)
        {
            await _vectorDbService.DeleteDocumentVectorsAsync(documentId);
            return Ok(new { message = $"Vectors for document {documentId} deleted successfully" });
        }
*/
    }
}
