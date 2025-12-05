using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Application.DTOs.Chat
{
    public class ChatItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime LastActiveAt { get; set; }
    }
}
