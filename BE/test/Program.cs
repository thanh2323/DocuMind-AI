/*using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.IPdf;
using DocuMind.Infrastructure.Extention;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("--- DocuMind Semantic Search Test Tool ---");

// 1. Setup Configuration
var configData = new Dictionary<string, string>
{
    {"ConnectionStrings:DefaultConnection", "Server=.;Database=DocuMindDb;Trusted_Connection=True;TrustServerCertificate=True;"},
    {"Ollama:BaseUrl", "http://localhost:11434"},
    {"Ollama:EmbeddingModel", "mxbai-embed-large"},
    {"Ollama:TimeoutSeconds", "120"}
};
var config = new ConfigurationBuilder()
    .AddInMemoryCollection(configData)
    .Build();

// 2. Setup Dependency Injection
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Warning); // Less noise
});
services.AddSingleton<IConfiguration>(config);

// Use the existing Infrastructure extension to register everything
// This allows us to test the actual DI setup and access internal services if needed
services.AddInfrastructure(config);

var provider = services.BuildServiceProvider();

// 3. Resolve Services
var pdfService = provider.GetRequiredService<IPdfProcessorService>();
var embeddingService = provider.GetRequiredService<IEmbeddingService>();

try
{
    // ==========================================
    // STEP 1: LOAD & EMBED FILE
    // ==========================================
    Console.Write("\n[?] Enter path to PDF file (or press Enter to use default 'D:\\tv.pdf'): ");
    var inputPath = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(inputPath)) inputPath = @"D:\GameProject_Report_NguyenChiThanh.pdf";

    if (!File.Exists(inputPath))
    {
        Console.WriteLine($"❌ File not found: {inputPath}");
        return;
    }

    Console.WriteLine($"\n[i] Processing file: {inputPath}...");
    var rawText = pdfService.ExtractCleanText(inputPath);
    Console.WriteLine($"    Extracted {rawText.Length} chars.");

    var chunks = pdfService.ChunkSemantic(rawText, 500, 50);
    Console.WriteLine($"    Created {chunks.Count} chunks.");

    Console.Write("    Generating embeddings... ");
    // EmbedChunksAsync returns IReadOnlyList<float[]>
    var vectors = await embeddingService.EmbedChunksAsync(chunks);
    Console.WriteLine("Done.");

    if (vectors.Count != chunks.Count)
    {
        Console.WriteLine("❌ Mismatch between chunks and vectors count!");
        return;
    }

    // Zip them together for searching
    var database = chunks.Zip(vectors, (text, vec) => new { Text = text, Vector = vec }).ToList();

    // ==========================================
    // STEP 2: INTERACTIVE Q&A
    // ==========================================
    Console.WriteLine("\n--- Verified & Ready for Querying ---");
    Console.WriteLine("Type a question to see related chunks. Type 'exit' to quit.\n");

    while (true)
    {
        Console.Write("> ");
        var query = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(query) || query.Trim().ToLower() == "exit") break;

        // 1. Embed query
        var queryVector = await embeddingService.EmbedChunkAsync(query);

        if (queryVector == null || queryVector.Length == 0)
        {
            Console.WriteLine("❌ Failed to embed query.");
            continue;
        }

        // 2. Rank by Cosine Similarity
        var results = database
            .Select(x => new
            {
                Similarity = CosineSimilarity(queryVector, x.Vector),
                Chunk = x.Text
            })
            .OrderByDescending(x => x.Similarity)
            .Take(3)
            .ToList();

        // 3. Display
        Console.WriteLine("\nTop 3 Relevant Chunks:");
        foreach (var res in results)
        {
            Console.WriteLine("========================================");
            Console.WriteLine($"Similarity: {res.Similarity:F4} | {ExpectationLabel(res.Similarity)}");
            Console.WriteLine("Chunk:");
            Console.WriteLine(res.Chunk);

            Console.WriteLine("========================================");
        }
        Console.WriteLine();
    }

}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ ERROR: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

// Math Helper
static double CosineSimilarity(float[] vectorA, float[] vectorB)
{
    double dot = 0, normA = 0, normB = 0;
    for (int i = 0; i < vectorA.Length; i++)
    {
        dot += vectorA[i] * vectorB[i];
        normA += Math.Pow(vectorA[i], 2);
        normB += Math.Pow(vectorB[i], 2);
    }
    return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
}
static string ExpectationLabel(double similarity)
{
    if (similarity >= 0.80) return "✅ Very Relevant (Expected)";
    if (similarity >= 0.65) return "🙂 Relevant";
    if (similarity >= 0.50) return "⚠️ Weak Match";
    return "❌ Irrelevant (Unexpected)";
}
*/