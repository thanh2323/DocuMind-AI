using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Admin;
using DocuMind.Application.DTOs.Common;

namespace DocuMind.Application.Interface.IUser
{
    public interface IAdminService
    {
        Task<ServiceResult<AdminDashboardStatsDto>> GetDashboardStats();
        Task<ServiceResult<List<UserAdminDto>>> GetAllUsers();
        Task<ServiceResult<bool>> LockUser(int userId);
        Task<ServiceResult<bool>> UnlockUser(int userId);
        Task<ServiceResult<bool>> DeleteUser(int userId);
    }
}
