using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Document;

namespace DocuMind.Application.DTOs.Chat
{
    public class SessionDto
    {
        public int Id { get; set; }
        public string? Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActiveAt { get; set; }
        public int MessageCount { get; set; }
        public List<DocumentItemDto> Documents { get; set; } = new();
    }
}
