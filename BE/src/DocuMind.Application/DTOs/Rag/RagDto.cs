using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Application.DTOs.Rag
{
   public class RagDto
    {
        public string Answer { get; set; } = string.Empty;
        public long ProcessingTimeMs { get; set; }
    }
}
