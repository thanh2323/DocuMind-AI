using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocuMind.Infrastructure.DTOs
{

    public class EmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = [];

        [JsonPropertyName("embeddings")] 
        public List<float[]>? Embeddings { get; set; }
    }
}
