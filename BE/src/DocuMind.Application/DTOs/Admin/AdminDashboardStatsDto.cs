using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Application.DTOs.Admin
{
    public class AdminDashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalChatSessions { get; set; }
        public long TotalStorageUsed { get; set; } // In bytes
    }
}
