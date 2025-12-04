using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.User;

namespace DocuMind.Application.Interface.IUser
{
    public interface IUserService
    {

        Task<ServiceResult<UserProfileDto>> GetProfile(int id);
        Task<ServiceResult<UserProfileDto>> UpdateProfile(int id, UpdateProfileDto dto);
        Task<ServiceResult<bool>> DeleteAccount(string id);

    }
}
