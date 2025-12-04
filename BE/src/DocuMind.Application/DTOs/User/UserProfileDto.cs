using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Application.DTOs.User
{
    public class UserProfileDto
    {
 
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalChats { get; set; }

    }
}
