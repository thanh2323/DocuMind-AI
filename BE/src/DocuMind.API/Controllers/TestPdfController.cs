using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.IPdf;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/test-pdf")]
public class TestPdfController : ControllerBase
{
    private readonly IPdfProcessorService _pdfService;
    private readonly IEmbeddingService _embeddingService;

    public TestPdfController(IPdfProcessorService pdfService, IEmbeddingService embeddingService)
    {
        _pdfService = pdfService;
        _embeddingService = embeddingService;
    }

    [HttpGet("embed")]
    public async Task<IActionResult> TestEmbedding(CancellationToken cancellationToken)
    {
        var path = @"D:\tv.pdf";

        if (!_pdfService.ValidatePdf(path))
            return BadRequest("Invalid PDF");

        var text = _pdfService.ExtractCleanText(path);

        var chunks = _pdfService.ChunkSemantic(text, 2000, 100);

        var embeddings = await _embeddingService.EmbedChunksAsync(chunks, cancellationToken);

        return Ok(new
        {
            textLength = text.Length,
            chunksCount = chunks.Count,
            embeddingsCount = embeddings.Count,
         
            sampleVector = embeddings.ToList()
        });
    }
}
