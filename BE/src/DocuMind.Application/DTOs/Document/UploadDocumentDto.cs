using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DocuMind.Application.DTOs.Document
{
   public class UploadDocumentDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
