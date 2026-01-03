using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Repositories
{
   public class UserRepository : GenericRepository<User>, IUserRepository
    {

        public UserRepository(SqlServer context) : base(context)
        {
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<List<(User User, int DocumentCount, int ChatCount)>> GetUsersWithStatsAsync()
        {
            var result = await _context.Users
                .Select(u => new 
                { 
                    User = u, 
                    DocCount = u.Documents.Count(), 
                    ChatCount = u.ChatSessions.Count() 
                })
                .ToListAsync();

            return result.Select(x => (x.User, x.DocCount, x.ChatCount)).ToList();
        }
    }
}
