using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Core.Interfaces.IEmbedding
{
    public interface IEmbeddingService
    {
        Task<float[]> EmbedChunkAsync(string chunk, CancellationToken ct = default);
         Task<List<float[]>> EmbedChunksAsync(IReadOnlyList<string> chunks, CancellationToken ct = default);
    }
}