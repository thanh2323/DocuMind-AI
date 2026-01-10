using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.User.Dashboard;
using DocuMind.Application.DTOs.Document;

namespace DocuMind.Application.Interface.IUser
{
    public interface IUserDashboardService
    {
        Task<ServiceResult<UserDashboardDto>> GetDashboardAsync(int id);
        Task<ServiceResult<PagedResult<DocumentItemDto>>> GetDocumentsPagedAsync(int userId, int page, int pageSize = 5);
    }
}
