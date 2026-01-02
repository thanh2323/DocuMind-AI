using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;
using DocuMind.Core.Enum;

namespace DocuMind.Core.Interfaces.IRepo
{
   public interface IDocumentRepository : IRepository<Document>
    {
        Task<List<Document>> GetDocumentsAsync(List<int> documentIds, int userId);

        Task<IEnumerable<Document>> GetPagedUserDocumentsAsync(int userId, int page, int pageSize);
        Task<int> CountUserDocumentsAsync(int userId);
        Task<IEnumerable<Document>> GetRecentDocumentsAsync(int userId, int take);

        Task<IEnumerable<Document>> GetByStatusAsync(DocumentStatus status);
        Task<Dictionary<DocumentStatus, int>> GetStatusCountsAsync(int userId);
    }
}
