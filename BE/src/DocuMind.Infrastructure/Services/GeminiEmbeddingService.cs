using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Infrastructure.DTOs;
using DocuMind.Infrastructure.DTOs.GeminiEmbed;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _client;
    private readonly string _apiKey;
    private readonly string _model;

    private const int BATCH_SIZE = 5;

    public GeminiEmbeddingService(IHttpClientFactory factory, IConfiguration config)
    {
        _client = factory.CreateClient("Gemini");
        _apiKey = config["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Missing Gemini API Key");

        _model = config["Gemini:EmbeddingModel"]
            ?? "models/gemini-embedding-1.0";
    }

    // ========== SINGLE CHUNK ==========
    public async Task<float[]> EmbedChunkAsync(
        string chunk,
        CancellationToken ct = default)
    {
        var request = new GeminiEmbeddingRequest
        {
            content =
            {
                parts =
                {
                    new GeminiPart { text = chunk }
                }
            }
        };


        var response = await _client.PostAsJsonAsync(
            $"models/{_model}:embedContent?key={_apiKey}",
            request,
            ct
        );

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<GeminiEmbeddingResponse>(ct);

        return result?.embedding.values ?? Array.Empty<float>();
    }

    // ========== MANY CHUNKS (CLIENT-SIDE BATCH) ==========
    public async Task<List<float[]>> EmbedChunksAsync(
        IReadOnlyList<string> chunks,
        CancellationToken ct = default)
    {
        if (chunks == null || chunks.Count == 0)
            return new List<float[]>();

        var allEmbeddings = new List<float[]>(chunks.Count);

        // Handle in batches of BATCH_SIZE
        for (int i = 0; i < chunks.Count; i += BATCH_SIZE)
        {
            // 1. Take 5 chunks from the list
            var currentBatch = chunks.Skip(i).Take(BATCH_SIZE).ToArray();

            var request = new GeminiBatchEmbeddingRequest();

            foreach (var chunk in currentBatch)
            {
                request.requests.Add(new GeminiEmbeddingRequest
                {
                    model = $"models/{_model}",
                    content = new GeminiContent
                    {
                        parts = { new GeminiPart { text = chunk } }
                    }
                });
            }

            try
            {
                // 3. Send request to Gemini API 
                var response = await _client.PostAsJsonAsync($"models/{_model}:batchEmbedContents?key={_apiKey}", request, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<GeminiBatchEmbeddingResponse>(ct);

                if (result?.embeddings != null)
                {
                    allEmbeddings.AddRange(result.embeddings.Select(e => e.values));
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
