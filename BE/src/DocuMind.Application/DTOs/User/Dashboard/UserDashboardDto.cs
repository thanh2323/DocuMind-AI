using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.DTOs.Document;

namespace DocuMind.Application.DTOs.User.Dashboard
{
    public class UserDashboardDto
    {
        public required UserDashboardStatisticsDto Statistics { get; set; } 
        public IEnumerable<DocumentItemDto> RecentDocuments { get; set; } = Enumerable.Empty<DocumentItemDto>();
        public IEnumerable<SessionDto> RecentChats { get; set; } = Enumerable.Empty<SessionDto>();
    }
}
