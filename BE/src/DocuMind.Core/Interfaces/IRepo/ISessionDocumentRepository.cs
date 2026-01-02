using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;

namespace DocuMind.Core.Interfaces.IRepo
{
    public interface ISessionDocumentRepository : IRepository<SessionDocument>
    {
        Task AddRangeAsync(IEnumerable<SessionDocument> entities);

    }
}
