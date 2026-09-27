using Examify.Services.OCR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;

namespace Examify.Controllers
{
    [Authorize]
    public class QuestionExtractorController : Controller
    {
        private readonly ILogger<QuestionExtractorController> _logger;
        private readonly GeminiOcrService _geminiOcr;
        private readonly DiagramDetectionService _diagramDetection;
        private readonly PdfToImageService _pdfToImage;
        private readonly GeminiModelService _geminiModel;
        private readonly IQuestionExtractorService _apiService;

        public QuestionExtractorController(ILogger<QuestionExtractorController> logger, GeminiOcrService geminiOcr, DiagramDetectionService diagramDetection, PdfToImageService pdfToImage, GeminiModelService geminiModel, IQuestionExtractorService apiService)
        {
            _logger = logger;
            _geminiOcr = geminiOcr;
            _diagramDetection = diagramDetection;
            _pdfToImage = pdfToImage;
            _geminiModel = geminiModel;
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ImageUpload()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListModels()
        {
            var models = await _geminiModel.ListAvailableModelsAsync();
            return Ok(models);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] int topicId, [FromForm] List<int> classIds)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("File size exceeds 10MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(ext))
                return BadRequest("Invalid image format. Only JPG, PNG, and WebP are allowed.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var imageBytes = ms.ToArray();

            var connectionString = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("AzureBlobStorage") ?? "UseDevelopmentStorage=true";
            var queueConnectionString = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("AzureServiceBus") ?? "Endpoint=sb://examify.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=123";
            var instituteId = HttpContext.User.FindFirst("InstituteId")?.Value ?? "0";

            // Push to Azure Service Bus for decoupled processing
            try
            {
                var serviceBusClient = new Azure.Messaging.ServiceBus.ServiceBusClient(queueConnectionString);
                var sender = serviceBusClient.CreateSender("ocr-queue");

                // Store image in Azure Blob Storage
                var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("ocr-images");
                await containerClient.CreateIfNotExistsAsync();

                var blobName = $"{Guid.NewGuid()}{ext}";
                var blobClient = containerClient.GetBlobClient(blobName);

                using var uploadStream = new MemoryStream(imageBytes);
                await blobClient.UploadAsync(uploadStream, overwrite: true);

                var payload = new
                {
                    FileName = file.FileName,
                    BlobName = blobName,
                    TopicId = topicId,
                    ClassIds = classIds,
                    InstituteId = instituteId
                };

                var msg = new Azure.Messaging.ServiceBus.ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(payload));
                await sender.SendMessageAsync(msg);

                return Ok(new { Message = "Image upload queued successfully for background OCR processing." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue image for OCR processing.");
                return StatusCode(500, "Failed to enqueue processing task.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadPdf(IFormFile file, [FromForm] int topicId, [FromForm] List<int> classIds)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (file.Length > 20 * 1024 * 1024)
                return BadRequest("File size exceeds 20MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".pdf")
                return BadRequest("Invalid file type. Only PDF documents are allowed.");

            var connectionString = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("AzureBlobStorage") ?? "UseDevelopmentStorage=true";
            var queueConnectionString = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetConnectionString("AzureServiceBus") ?? "Endpoint=sb://examify.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=123";
            var instituteId = HttpContext.User.FindFirst("InstituteId")?.Value ?? "0";

            try
            {
                var serviceBusClient = new Azure.Messaging.ServiceBus.ServiceBusClient(queueConnectionString);
                var sender = serviceBusClient.CreateSender("ocr-queue-pdf");

                // Store PDF in Azure Blob Storage
                var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("ocr-pdfs");
                await containerClient.CreateIfNotExistsAsync();

                var blobName = $"{Guid.NewGuid()}{ext}";
                var blobClient = containerClient.GetBlobClient(blobName);

                using var uploadStream = file.OpenReadStream();
                await blobClient.UploadAsync(uploadStream, overwrite: true);

                var payload = new
                {
                    FileName = file.FileName,
                    BlobName = blobName,
                    TopicId = topicId,
                    ClassIds = classIds,
                    InstituteId = instituteId
                };

                var msg = new Azure.Messaging.ServiceBus.ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(payload));
                await sender.SendMessageAsync(msg);

                return Ok(new { Message = "PDF upload queued successfully for background OCR processing." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue PDF for OCR processing.");
                return StatusCode(500, "Failed to enqueue processing task.");
            }
        }
    }
}
