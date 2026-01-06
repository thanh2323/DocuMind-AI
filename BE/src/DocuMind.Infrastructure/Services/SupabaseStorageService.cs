using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Interfaces.IStorage;
using Supabase.Storage;

namespace DocuMind.Infrastructure.Services
{
    class SupabaseStorageService : IStorageService
    {
        private readonly Supabase.Client _client;

        public SupabaseStorageService(Supabase.Client client)
        {
            _client = client;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, int userId)
        {
            var path = $"documents/{userId}/{Guid.NewGuid()}_{fileName}";
            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                await _client.Storage
                    .From("Documents")
                    .Upload(fileBytes, path);
            }


            return path;
        }

        public async Task DeleteAsync(string filePath)
        {
            await _client.Storage
                .From("Documents")
                .Remove(filePath);
        }

        public async Task<Stream> GetFileStreamAsync(string filePath)
        {
            var bytes = await _client.Storage
              .From("Documents")
              .Download(filePath, (TransformOptions?)null);

            return new MemoryStream(bytes, writable: false);
        }
    }
}
