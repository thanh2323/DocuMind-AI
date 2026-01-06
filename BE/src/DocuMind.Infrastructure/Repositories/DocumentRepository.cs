using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.User.Dashboard;
using DocuMind.Core.Entities;
using DocuMind.Core.Enum;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Repositories
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(SqlServer context) : base(context)
        {
        }

      /*  public async Task<List<Document>> GetAllUserDocumentsAsync(int userId)
        {
            return await _dbSet.Where(d => d.UserId == userId).ToListAsync();
        }*/

        public async Task<IEnumerable<Document>> GetByStatusAsync(DocumentStatus status)
        {
            return await _context.Documents
                .Where(d => d.Status == status)
                .ToListAsync();
        }

        public async Task<Dictionary<DocumentStatus, int>> GetStatusCountsAsync(int userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .GroupBy(d => d.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<int> CountUserDocumentsAsync(int userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .CountAsync();
        }
        public async Task<IEnumerable<Document>> GetRecentDocumentsAsync(int userId, int take)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetPagedUserDocumentsAsync(int userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt);

            return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        }

        public async Task<List<Document>> GetDocumentsAsync(List<int> ids, int userId)
        {
            return await _context.Documents
                .Where(d => ids.Contains(d.Id) && d.UserId == userId)
                .ToListAsync();
        }
    }
}
