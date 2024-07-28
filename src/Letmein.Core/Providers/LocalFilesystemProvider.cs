using System;
using System.Collections.Generic;
using CloudFileStore;
using System.Threading.Tasks;
using System.IO;

namespace Letmein.Core.Providers
{
    public class LocalFilesystemProvider : IStorageProvider
    {
        public LocalFilesystemProvider()
        {
            if (!Directory.Exists("storage"))
            {
                Directory.CreateDirectory("storage");
            }
        }

        private string GetFullPath(string filename)
        {
            return Path.Combine("storage", filename);
        }

        public Task DeleteFileAsync(string filename)
        {
            string fullPath = GetFullPath(filename);
            File.Delete(fullPath);

            return Task.CompletedTask;
        }

        public Task<bool> FileExistsAsync(string filename)
        {
            bool exists = File.Exists(GetFullPath(filename));
            return Task.FromResult<bool>(exists);;
        }

        public Task<IEnumerable<string>> ListFilesAsync(int pageSize = 100, bool pagingEnabled = true)
        {
            string[] files = Directory.GetFiles("storage");
            return Task.FromResult<IEnumerable<string>>(files);
        }

        public Task<string> LoadTextFileAsync(string filename)
        {
            return File.ReadAllTextAsync(GetFullPath(filename));
        }

        public Task SaveTextFileAsync(string filePath, string fileContent, string contentType = "text/plain")
        {
            return File.WriteAllTextAsync(GetFullPath(filePath), fileContent);
        }
    }
}