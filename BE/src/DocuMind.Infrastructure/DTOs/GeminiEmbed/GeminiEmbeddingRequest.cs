using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Infrastructure.DTOs.GeminiEmbed
{

    public class GeminiBatchEmbeddingRequest
    {
        public List<GeminiEmbeddingRequest> requests { get; set; } = new();
    }
    public class GeminiEmbeddingRequest
    {
        public string model { get; set; } = default!;
        public GeminiContent content { get; set; } = new();
    }

    public class GeminiContent
    {
        public List<GeminiPart> parts { get; set; } = new();
    }

    public class GeminiPart
    {
        public string text { get; set; } = string.Empty;
    }

}
