using DocuMind.Application.DTOs.Document;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.ILLM;
using DocuMind.Core.Interfaces.IPdf;
using DocuMind.Core.Interfaces.IVectorDb;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/test-pdf")]
public class TestPdfController : ControllerBase
{
    private readonly IPdfProcessorService _pdfService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorDbService _qdrantService;
    private readonly IDocumentService _documentService;

    private readonly ILlmService _llm;

    public TestPdfController(ILlmService llm,IDocumentService documentService, IPdfProcessorService pdfService, IEmbeddingService embeddingService, IVectorDbService qdrantService)
    {
        _llm = llm;
        _documentService = documentService;
        _pdfService = pdfService;
        _embeddingService = embeddingService;
        _qdrantService = qdrantService;
    }

    [HttpGet("embed")]
    /* public async Task<IActionResult> TestEmbedding(CancellationToken cancellationToken)
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
     }*/

    [HttpPost("init-data")]
    public async Task<IActionResult> InitData()
    {
        // A. Tạo Collection
        await _qdrantService.InitializeCollectionAsync();

        // B. Giả lập dữ liệu
        int docId = 999; // ID giả
        var chunks = new List<string>
        {
            "Mèo thích ăn cá",
            "Chó thích gặm xương",
            "Lập trình viên thích uống cà phê"
        };
        // C. Giả lập Vector (Size 1024)
        // Lưu ý: Vector này là random nên tìm kiếm sẽ không chuẩn về ngữ nghĩa, 
        // nhưng test được xem có lưu vào DB được không.
        var vectors = new List<float[]>();
        var random = new Random();

        for (int i = 0; i < chunks.Count; i++)
        {
            float[] vec = new float[1024]; // Phải đúng size trong config
            for (int j = 0; j < 1024; j++) vec[j] = (float)random.NextDouble();
            vectors.Add(vec);
        }

        // D. Lưu vào Qdrant
        await _qdrantService.UpsertVectorsAsync(docId, chunks, vectors);

        return Ok("Đã tạo Collection và Insert 3 vector giả thành công!");
    }

    // 2. API Search thử
    [HttpGet("search")]
    public async Task<IActionResult> Search()
    {
        // Tạo 1 vector query ngẫu nhiên (để test code chạy không lỗi)
        float[] queryVector = new float[1024];
        var random = new Random();
        for (int j = 0; j < 1024; j++) queryVector[j] = (float)random.NextDouble();

        // Gọi hàm Search
        // Test filter: Chỉ tìm trong docId 999
        var results = await _qdrantService.SearchSimilarAsync(queryVector, new List<int> { 999 });

        return Ok(results);
    }

    // 3. API Xóa thử
    [HttpDelete("delete/{docId}")]
    public async Task<IActionResult> Delete(int docId)
    {
        await _qdrantService.DeleteDocumentVectorsAsync(docId);
        return Ok($"Đã xóa vector của docId {docId}");
    }

  /*  [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentDto dto)
    {
        int userId = 1;

        var result = await _documentService.UploadDocument(dto, userId);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Data);
    }*/
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] PromptDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt required!");

        var result = await _llm.AskAsync(request.Prompt, ct);

        if (string.IsNullOrWhiteSpace(result))
            return Ok("No LLM response returned");

        return Ok(result);
    }


    public class PromptDto
    {
        public string Prompt { get; set; } = "";
    }
}


