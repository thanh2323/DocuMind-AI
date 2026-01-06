using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Application.DTOs.Chat
{
   public class ChatResponseDto
    {
        public MessageDto UserMessage { get; set; } = null!;
        public MessageDto BotMessage { get; set; } = null!;
        public long ProcessingTimeMs { get; set; }
    }
}
