using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Infrastructure.DTOs.GeminiEmbed
{

    public class GeminiBatchEmbeddingResponse
    {
        public List<GeminiEmbedding> embeddings { get; set; } = new(); // Batch of embeddings
    }
    public class GeminiEmbeddingResponse
    {
        public GeminiEmbedding embedding { get; set; } = new(); // Single embedding
    }

    public class GeminiEmbedding
    {   
        public float[] values { get; set; } = Array.Empty<float>();
    }
}
