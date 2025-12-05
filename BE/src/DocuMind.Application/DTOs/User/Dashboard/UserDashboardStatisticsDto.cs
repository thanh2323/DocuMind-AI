using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Enum;

namespace DocuMind.Application.DTOs.User.Dashboard
{
    public class UserDashboardStatisticsDto
    {
        public int TotalDocuments { get; set; }
    
        public int TotalChats { get; set; }
        public required  Dictionary<DocumentStatus, int> StatusCounts { get; set; }
    }
}
