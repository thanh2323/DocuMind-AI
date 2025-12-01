using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;

namespace DocuMind.Core.Interfaces.IAuth
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        //int? ValidateToken(string token);
    }
}
