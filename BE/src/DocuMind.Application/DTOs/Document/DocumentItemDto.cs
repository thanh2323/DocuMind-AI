using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Enum;

namespace DocuMind.Application.DTOs.Document
{
    public class DocumentItemDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; } 

        public DocumentStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();

        public DateTime CreatedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }
    }
}
