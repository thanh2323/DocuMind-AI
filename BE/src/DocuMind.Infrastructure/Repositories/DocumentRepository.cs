using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;
using DocuMind.Core.Enum;
using DocuMind.Core.Interfaces;
using DocuMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Repositories
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(SqlServer context) : base(context)
        {
        }

        public async Task<IEnumerable<Document>> GetByStatusAsync(DocumentStatus status)
        {
            return await _context.Documents
                .Where(d => d.Status == status)
                .ToListAsync();
        }

        public async Task<int> GetUserDocumentCountAsync(int userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .CountAsync();
        }

        public async Task<IEnumerable<Document>> GetUserDocumentsAsync(int userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Documents
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt);

            return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        }
    }
}
