using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Infrastructure.Data;

namespace DocuMind.Infrastructure.Repositories
{
    public class SessionDocumentRepository : GenericRepository<SessionDocument>, ISessionDocumentRepository
    {
        public SessionDocumentRepository(SqlServer context) : base(context)
        {
        }

        public async Task AddRangeAsync(IEnumerable<SessionDocument> entities)
        {
            await _context.SessionDocuments.AddRangeAsync(entities);
        }

    }
}
