using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Core.Interfaces.IEmbedding
{
    public interface IEmbeddingClient
    {
        Task<float[]> EmbedContentAsync(string text, CancellationToken cancellationToken = default);
        Task<List<float[]>> EmbedBatchAsync(List<string> texts, CancellationToken cancellationToken);
    }
}
