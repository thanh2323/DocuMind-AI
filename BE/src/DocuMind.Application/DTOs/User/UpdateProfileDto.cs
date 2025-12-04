using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Application.DTOs.User
{
   public class UpdateProfileDto
    {
      
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;
    }
}
