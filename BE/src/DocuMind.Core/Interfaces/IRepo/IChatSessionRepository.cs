using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;

namespace DocuMind.Core.Interfaces.IRepo
{
    public interface IChatSessionRepository : IRepository<ChatSession>
    {
        Task<ChatSession?> GetWithDocumentsAsync(int sessionId);
        Task<IEnumerable<ChatSession>> GetByUserIdAsync(int userId);
        Task<ChatSession?> GetWithRecentMessagesAsync(int sessionId, int count);
    }
}
