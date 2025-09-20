using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Examify.Common;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;

namespace Examify.Helpers
{
    public interface IFileService
    {
        Task<string> SaveUploadedFile(IFormFile file);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppSettings _settings;

        public FileService(IWebHostEnvironment env, IOptions<AppSettings> options)
        {
            _env = env; 
            _settings = options.Value;
        }
        public async Task<string> SaveUploadedFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            string fname = string.Empty;

            var uploadPath = Path.Combine(_env.WebRootPath, _settings.UploadPath);

            // Create folder if not exists
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Full file path
            var newName = Guid.NewGuid() + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadPath, newName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return newName; 
        }
    }
}