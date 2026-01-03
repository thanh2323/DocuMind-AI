using DocuMind.Core.Entities;
using DocuMind.Core.Enum;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Repositories
{
    public class ChatSessionRepository : GenericRepository<ChatSession> , IChatSessionRepository
    {
        public ChatSessionRepository(SqlServer context) : base(context)
        {
        }

        public async Task<IEnumerable<ChatSession>> GetByUserIdAsync(int userId)
        {
            return await _context.ChatSessions
                .Where(cs => cs.UserId == userId)
                .ToListAsync();
        }

        public async Task<ChatSession?> GetWithDocumentsAsync(int sessionId)
        {
            return await _context.ChatSessions
                .AsNoTracking()
                .Include(cs => cs.SessionDocuments)
                    .ThenInclude(sd => sd.Document)
                .FirstOrDefaultAsync(cs => cs.Id == sessionId);

        }

        public async Task<ChatSession?> GetWithRecentMessagesAsync(int sessionId, int count)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.Id == sessionId);

            if (session != null)
            {
                session.Messages = await _context.ChatMessages
                    .Where(m => m.SessionId == sessionId)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(count)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }

            return session;
        }
        public async Task<List<ChatSession>> GetRecentChatsAsync(int userId, int take = 5)
        {
            return await _context.ChatSessions
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(take)
                .ToListAsync();
        }


    }
}
