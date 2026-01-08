using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Application.DTOs.Chat
{
    public class CreateSessionDto
    {
        
        [StringLength(200, MinimumLength = 3)]
        public string? Title { get; set; }

    }
}
