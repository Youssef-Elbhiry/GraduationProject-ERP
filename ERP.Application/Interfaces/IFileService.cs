using Microsoft.AspNetCore.Http;

namespace ERP.Application.Interfaces
{
    public interface IFileService
    {
        public Task<string> UploadFileAsync(IFormFile file, string location);
        public bool DeleteFile(string filePath);
    }
}
