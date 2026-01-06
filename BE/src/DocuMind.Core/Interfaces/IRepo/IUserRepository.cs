using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;

namespace DocuMind.Core.Interfaces.IRepo
{
    public  interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<List<(User User, int DocumentCount, int ChatCount)>> GetUsersWithStatsAsync();
    }
}
