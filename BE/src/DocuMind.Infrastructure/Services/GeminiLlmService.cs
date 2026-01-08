using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Interfaces.ILLM;
using DocuMind.Infrastructure.DTOs;
using Google.GenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;



namespace DocuMind.Infrastructure.Services
{
    class GeminiLlmService : ILlmService
    {
        private readonly HttpClient _client;

        private readonly ILogger<GeminiLlmService> _logger;
        private readonly string _model;
        private readonly string _apiKey;

        public GeminiLlmService(IHttpClientFactory factory, IConfiguration configuration, ILogger<GeminiLlmService> logger)
        {
            _apiKey = configuration["Gemini:ApiKey"]!;
            _client = factory.CreateClient("Gemini");
            _logger = logger;
            _model = configuration["Gemini:Model"]!;
            _logger.LogInformation("Gemini LLM Service initialized with model: {Model}", _model);
        }

        public async Task<string> AskAsync(string prompt, CancellationToken ct = default)
        {
            var request = new
            {
                contents = new[]
                {
                new {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
                }
            };

            var response = await _client.PostAsJsonAsync($"models/{_model}:generateContent?key={_apiKey}", request, ct);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct);


            var builder = new StringBuilder();

            foreach (var candidate in json.Candidates)
            {
                foreach (var part in candidate.Content.Parts)
                {
                    builder.AppendLine(part.Text);
                    builder.AppendLine();
                }
            }

            return builder.ToString().Trim();
        }
    }


}
