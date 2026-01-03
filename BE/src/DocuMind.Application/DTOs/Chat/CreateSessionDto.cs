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

        [Required]
        [MinLength(1, ErrorMessage = "At least one document is required")]
        public List<int> DocumentIds { get; set; } = new();
    }
}
