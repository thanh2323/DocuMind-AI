using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Core.Interfaces.IStorage
{
    public interface IStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, int userId);
        Task DeleteAsync(string filePath);
        Task<Stream> GetFileStreamAsync(string filePath);
    }
}
