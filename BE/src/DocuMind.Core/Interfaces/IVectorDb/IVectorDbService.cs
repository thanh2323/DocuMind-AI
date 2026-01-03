using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Core.Interfaces.IVectorDb
{
    public interface IVectorDbService
    {
        Task InitializeCollectionAsync();
        Task UpsertVectorsAsync(int documentId, List<string> chunks, List<float[]> vectors);
        Task<List<SearchResult>> SearchSimilarAsync(float[] queryVector, List<int>? documentIds = null, int limit = 5);
        Task DeleteDocumentVectorsAsync(int documentId);
        Task<bool> CollectionExistsAsync();
    }

    public class SearchResult
    {
        public int DocumentId { get; set; }
        public string ChunkText { get; set; } = string.Empty;
        public float Score { get; set; }
        public int ChunkIndex { get; set; }
    }

}
