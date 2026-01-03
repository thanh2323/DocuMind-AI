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
using static System.Net.WebRequestMethods;


namespace DocuMind.Infrastructure.Services
{
    class GeminiLlmService : ILlmService
    {
        private readonly HttpClient _httpClient;

        private readonly ILogger<GeminiLlmService> _logger;
        private readonly string _model;
        private readonly string _key;

        public GeminiLlmService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiLlmService> logger)
        {
            _key = configuration["Gemini:ApiKey"]!;
            _httpClient = httpClient;
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

            var response = await _httpClient.PostAsJsonAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_key}",
                request
            );

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct);

            // ghép tất cả parts thành text nhiều dòng
            var builder = new StringBuilder();

            foreach (var candidate in json.Candidates)
            {
                foreach (var part in candidate.Content.Parts)
                {
                    builder.AppendLine(part.Text);
                    builder.AppendLine(); // thêm khoảng trắng giữa các đoạn
                }
            }

            return builder.ToString().Trim();
        }
    }

   
}
