using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Application.Options
{
    public class FileUploadOptions
    {
        public string[] AllowedExtensions { get; set; } = [".pdf"];
        public int MaxFileSizeMB { get; set; } = 50;
        public string UploadPath { get; set; } = "Uploads";
    }
}
