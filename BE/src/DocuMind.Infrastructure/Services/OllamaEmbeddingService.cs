using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Infrastructure.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _client;
    private readonly string _model;
    private const int BATCH_SIZE = 5;

    public OllamaEmbeddingService(
        IHttpClientFactory factory,
        IConfiguration config)
    {
        _client = factory.CreateClient("Ollama");
        _model = config["Ollama:EmbeddingModel"] ?? "mxbai-embed-large";
    }

    // ========== SINGLE CHUNK ==========
    public async Task<float[]> EmbedChunkAsync(
        string chunk,
        CancellationToken ct = default)
    {
        var response = await _client.PostAsJsonAsync("/api/embeddings", new
        {
            model = _model,
            prompt = chunk
        }, ct);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(ct);

        if (result?.Embedding == null || result.Embedding.Length == 0)
        {
           
            return Array.Empty<float>();
        }

        return result.Embedding;
    }

    // ========== MANY CHUNKS (SEQUENTIAL) ==========
    public async Task<IReadOnlyList<float[]>> EmbedChunksAsync(IReadOnlyList<string> chunks,CancellationToken ct = default)
    {
        if (chunks == null || chunks.Count == 0)
            return Array.Empty<float[]>();

        var allEmbeddings = new List<float[]>(chunks.Count);



        // Handle in batches of BATCH_SIZE
        for (int i = 0; i < chunks.Count; i += BATCH_SIZE)
        {
            // 1. Take 5 chunks from the list
            var currentBatch = chunks.Skip(i).Take(BATCH_SIZE).ToArray();

            // 2. create payload for the batch (5 chunks)
            var payload = new
            {
                model = _model,
                input = currentBatch
            };

            try
            {
                // 3. Send request to Ollama API 
                var response = await _client.PostAsJsonAsync("/api/embed", payload, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(ct);
      
                if (result?.Embeddings != null)
                {
                    allEmbeddings.AddRange(result.Embeddings);
                }
            }
            catch (Exception ex)
            {
                // Log the error so Hangfire can retry
                // You can replace this with your preferred logging framework
                Console.Error.WriteLine($"Error in batch {i}: {ex.Message}");
  
            }
        }
    
        return allEmbeddings;
    }
}

