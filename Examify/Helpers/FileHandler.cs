using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Examify.Common;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace Examify.Helpers
{
    public interface IFileService
    {
        Task<string> SaveUploadedFile(IFormFile file);
        Task<string> SaveCompressedImage(IFormFile file, string uploadFolder);
        void DeleteFile(string fileName, string uploadFolder);
        void DeleteFiles(List<string> fileNames, string uploadFolder);
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

        public async Task<string> SaveCompressedImage(IFormFile file, string uploadFolder)
        {
            if (file == null || file.Length == 0)
                return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException("Only image files are allowed.");

            var uploadPath = Path.Combine(_env.WebRootPath, uploadFolder);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid().ToString() + ".webp";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var inputStream = file.OpenReadStream())
            using (var image = Image.Load(inputStream))
            {
                if (image.Width > 1920)
                {
                    var ratio = 1920.0 / image.Width;
                    var newHeight = (int)(image.Height * ratio);
                    image.Mutate(x => x.Resize(1920, newHeight));
                }

                var encoder = new WebpEncoder { Quality = 85 };
                await image.SaveAsync(filePath, encoder);
            }

            return fileName;
        }

        public void DeleteFile(string fileName, string uploadFolder)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var filePath = Path.Combine(_env.WebRootPath, uploadFolder, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void DeleteFiles(List<string> fileNames, string uploadFolder)
        {
            if (fileNames == null || !fileNames.Any()) return;

            foreach (var fileName in fileNames)
            {
                DeleteFile(fileName, uploadFolder);
            }
        }
    }
}