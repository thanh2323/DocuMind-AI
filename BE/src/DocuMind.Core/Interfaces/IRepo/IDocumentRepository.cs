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
        Task<IEnumerable<Document>> GetUserDocumentsAsync(int userId, int page, int pageSize);
        Task<int> GetUserDocumentCountAsync(int userId);
        Task<IEnumerable<Document>> GetByStatusAsync(DocumentStatus status);
    }
}
