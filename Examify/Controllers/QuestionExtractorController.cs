using Examify.Services.OCR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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
            return Content(models, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] int topicId, [FromForm] List<int> classIds)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var imageBytes = ms.ToArray();

            var result = await _geminiOcr.ExtractFromImageAsync(imageBytes, file.ContentType);
 
                var jsonParser = HttpContext.RequestServices.GetRequiredService<GeminiJsonParserService>();
                var (questions, _) = jsonParser.ParseJsonResponse(result);

                _logger.LogInformation($"Parsed {questions.Count} questions");

                var allDetected = _diagramDetection.DetectDiagrams(imageBytes);
                using var originalImage = Image.Load(imageBytes);
                
                _logger.LogInformation($"OpenCV detected {allDetected.Count} regions:");
                foreach (var d in allDetected)
                {
                    _logger.LogInformation($"  Region: {d.WidthPercent:F1}% x {d.HeightPercent:F1}%");
                }
                
                var detectedDiagrams = allDetected.Where(d => d.WidthPercent < 30 && d.HeightPercent < 30).ToList();
                _logger.LogInformation($"Filtered to {detectedDiagrams.Count} diagrams (width<30%, height<30%)");

                var imageArea = originalImage.Width * originalImage.Height;
                bool hasOversizedDiagram = detectedDiagrams.Any(d => 
                {
                    var diagramArea = d.Width * d.Height;
                    var percentage = (double)diagramArea / imageArea * 100;
                    _logger.LogInformation($"Diagram size: {d.Width}x{d.Height} = {percentage:F1}% of image");
                    return diagramArea > imageArea * 0.20;
                });
                
                if (hasOversizedDiagram)
                {
                    _logger.LogWarning("Detected oversized diagram (likely includes text), skipping extraction");
                }
                
                if (detectedDiagrams.Count > 0 && !hasOversizedDiagram)
                {
                    var diagramFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "QuesionUploads");
                    Directory.CreateDirectory(diagramFolder);

                    var questionsWithDiagrams = questions.Where(q => q.Diagrams != null && q.Diagrams.Count > 0).ToList();
                    _logger.LogInformation($"Gemini says {questionsWithDiagrams.Count} questions have diagrams:");
                    foreach (var q in questionsWithDiagrams)
                    {
                        _logger.LogInformation($"  Q{q.QuestionNumber} has {q.Diagrams.Count} diagram(s)");
                    }
                    
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    
                    if (questionsWithDiagrams.Count > 0)
                    {
                        _logger.LogInformation($"OpenCV detected: {detectedDiagrams.Count}, Questions with diagrams: {questionsWithDiagrams.Count}");
                        _logger.LogInformation($"OpenCV Y-positions: {string.Join(", ", detectedDiagrams.Select(d => $"{d.YPercent:F1}%"))}");
                        
                        var usedDiagrams = new HashSet<int>();
                        
                        foreach (var question in questionsWithDiagrams)
                        {
                            // Separate question diagrams from option diagrams
                            var questionDiagrams = question.Diagrams
                                .Where(d => !d.Description.Contains("Option", StringComparison.OrdinalIgnoreCase))
                                .ToList();
                            
                            if (questionDiagrams.Count == 0)
                            {
                                _logger.LogInformation($"Q{question.QuestionNumber}: Only has option diagrams, skipping question-level processing");
                                continue;
                            }
                            
                            _logger.LogInformation($"Processing Q{question.QuestionNumber}: {questionDiagrams.Count} question diagram(s)");
                            
                            var geminiYPositions = questionDiagrams.Select(d => d.YPercent).OrderBy(y => y).ToList();
                            _logger.LogInformation($"  Gemini Y-positions: {string.Join(", ", geminiYPositions.Select(y => $"{y:F1}%"))}");
                            
                            bool geminiCoordsValid = geminiYPositions.All(y => y >= 0 && y <= 100);
                            
                            int diagramsForThisQuestion = 0;
                            
                            if (geminiCoordsValid && questionsWithDiagrams.Count > 1)
                            {
                                // Multiple questions: use spatial matching
                                foreach (var geminiY in geminiYPositions)
                                {
                                    var closestDiagram = detectedDiagrams
                                        .Select((d, idx) => new { Diagram = d, Index = idx, Distance = Math.Abs(d.YPercent - geminiY) })
                                        .Where(x => !usedDiagrams.Contains(x.Index) && x.Distance < 20)
                                        .OrderBy(x => x.Distance)
                                        .FirstOrDefault();
                                    
                                    if (closestDiagram != null)
                                    {
                                        var diagram = closestDiagram.Diagram;
                                        _logger.LogInformation($"  Diagram {diagramsForThisQuestion}: Using OpenCV[{closestDiagram.Index}] at Y={diagram.YPercent:F1}% (Gemini Y={geminiY:F1}%, distance={closestDiagram.Distance:F1}%)");
                                        using var cropped = originalImage.Clone(ctx => ctx.Crop(new Rectangle(diagram.X, diagram.Y, diagram.Width, diagram.Height)));
                                        var fileName = $"q{question.QuestionNumber}_d{diagramsForThisQuestion}_{timestamp}.png";
                                        await cropped.SaveAsPngAsync(Path.Combine(diagramFolder, fileName));
                                        
                                        question.QuestionText += $" <img src='/QuesionUploads/{fileName}' alt='Diagram {diagramsForThisQuestion + 1}' style='max-width:400px;display:block;margin:10px 0;'/>";
                                        _logger.LogInformation($"  Saved as {fileName}");
                                        usedDiagrams.Add(closestDiagram.Index);
                                        diagramsForThisQuestion++;
                                    }
                                }
                            }
                            else
                            {
                                // Gemini coords invalid OR single question: use sequential assignment
                                _logger.LogWarning($"  Gemini coordinates invalid, using sequential assignment");
                                
                                int diagramsToAssign = questionsWithDiagrams.Count == 1 ? detectedDiagrams.Count : questionDiagrams.Count;
                                
                                for (int i = 0; i < diagramsToAssign && usedDiagrams.Count < detectedDiagrams.Count; i++)
                                {
                                    var nextIndex = Enumerable.Range(0, detectedDiagrams.Count).First(idx => !usedDiagrams.Contains(idx));
                                    var diagram = detectedDiagrams[nextIndex];
                                    _logger.LogInformation($"  Diagram {diagramsForThisQuestion}: Using OpenCV[{nextIndex}] at Y={diagram.YPercent:F1}%");
                                    using var cropped = originalImage.Clone(ctx => ctx.Crop(new Rectangle(diagram.X, diagram.Y, diagram.Width, diagram.Height)));
                                    var fileName = $"q{question.QuestionNumber}_d{diagramsForThisQuestion}_{timestamp}.png";
                                    await cropped.SaveAsPngAsync(Path.Combine(diagramFolder, fileName));
                                    
                                    question.QuestionText += $" <img src='/QuesionUploads/{fileName}' alt='Diagram {diagramsForThisQuestion + 1}' style='max-width:400px;display:block;margin:10px 0;'/>";
                                    _logger.LogInformation($"  Saved as {fileName}");
                                    usedDiagrams.Add(nextIndex);
                                    diagramsForThisQuestion++;
                                }
                            }

                            _logger.LogInformation($"  Q{question.QuestionNumber} got {diagramsForThisQuestion} diagram(s)");
                        }
                        _logger.LogInformation($"Diagram mapping complete: used {usedDiagrams.Count} of {detectedDiagrams.Count} OpenCV diagrams");
                        
                        // Process option diagrams for ALL questions
                        _logger.LogInformation("=== Processing Option Diagrams ===");
                        foreach (var question in questions)
                        {
                            if (question.Diagrams == null || question.Diagrams.Count == 0) continue;
                            
                            var optionDiagrams = question.Diagrams
                                .Where(d => d.Description.Contains("Option", StringComparison.OrdinalIgnoreCase))
                                .ToList();
                            
                            if (optionDiagrams.Count > 0)
                            {
                                _logger.LogInformation($"Q{question.QuestionNumber} has {optionDiagrams.Count} option diagram(s)");
                                
                                var maxOptNum = optionDiagrams
                                    .Select(d => System.Text.RegularExpressions.Regex.Match(d.Description, @"option\s+([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                    .Where(m => m.Success)
                                    .Select(m => int.Parse(m.Groups[1].Value))
                                    .DefaultIfEmpty(0)
                                    .Max();
                                
                                while (question.Options.Count < maxOptNum)
                                {
                                    question.Options.Add(new OcrOption { OptionNumber = question.Options.Count + 1, OptionText = "" });
                                }
                                
                                bool optionCoordsValid = optionDiagrams.All(d => d.YPercent >= 0 && d.YPercent <= 100);
                                
                                if (!optionCoordsValid)
                                {
                                    _logger.LogWarning($"  Option diagram coordinates invalid, using sequential assignment");
                                    var sortedOptions = optionDiagrams
                                        .Select(d => new { Diag = d, Match = System.Text.RegularExpressions.Regex.Match(d.Description, @"option\s+([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase) })
                                        .Where(x => x.Match.Success)
                                        .OrderBy(x => int.Parse(x.Match.Groups[1].Value))
                                        .ToList();
                                    
                                    for (int i = 0; i < sortedOptions.Count && usedDiagrams.Count < detectedDiagrams.Count; i++)
                                    {
                                        var nextIndex = Enumerable.Range(0, detectedDiagrams.Count).First(idx => !usedDiagrams.Contains(idx));
                                        var diagram = detectedDiagrams[nextIndex];
                                        var optNum = int.Parse(sortedOptions[i].Match.Groups[1].Value);
                                        
                                        using var cropped = originalImage.Clone(ctx => ctx.Crop(new Rectangle(diagram.X, diagram.Y, diagram.Width, diagram.Height)));
                                        var fileName = $"q{question.QuestionNumber}_opt{optNum}_{timestamp}.png";
                                        await cropped.SaveAsPngAsync(Path.Combine(diagramFolder, fileName));
                                        
                                        question.Options[optNum - 1].OptionImageUrl = $"/QuesionUploads/{fileName}";
                                        _logger.LogInformation($"  Option {optNum}: {fileName} (OpenCV[{nextIndex}])");
                                        usedDiagrams.Add(nextIndex);
                                    }
                                }
                                else
                                {
                                    foreach (var optDiag in optionDiagrams)
                                    {
                                        _logger.LogInformation($"  Processing: {optDiag.Description}");
                                        var match = System.Text.RegularExpressions.Regex.Match(optDiag.Description, @"option\s+([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                        if (match.Success && int.TryParse(match.Groups[1].Value, out int optNum))
                                        {
                                            var closestDiagram = detectedDiagrams
                                                .Select((d, idx) => new { Diagram = d, Index = idx, Distance = Math.Abs(d.YPercent - optDiag.YPercent) })
                                                .Where(x => !usedDiagrams.Contains(x.Index) && x.Distance < 20)
                                                .OrderBy(x => x.Distance)
                                                .FirstOrDefault();
                                            
                                            if (closestDiagram != null)
                                            {
                                                var diagram = closestDiagram.Diagram;
                                                using var cropped = originalImage.Clone(ctx => ctx.Crop(new Rectangle(diagram.X, diagram.Y, diagram.Width, diagram.Height)));
                                                var fileName = $"q{question.QuestionNumber}_opt{optNum}_{timestamp}.png";
                                                await cropped.SaveAsPngAsync(Path.Combine(diagramFolder, fileName));
                                                
                                                question.Options[optNum - 1].OptionImageUrl = $"/QuesionUploads/{fileName}";
                                                _logger.LogInformation($"  Option {optNum}: {fileName}");
                                                usedDiagrams.Add(closestDiagram.Index);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"OpenCV detection unreliable ({detectedDiagrams.Count} diagrams), skipping diagram extraction");
                }

                // Convert to QuestionModel and save to DB
                var questionModels = questions.Select(q => new DataModel.QuestionModel
                {
                    QuestionEnglish = q.QuestionText,
                    TopicId = topicId,
                    ClassIds = classIds,
                    QuestionTypeId = 1,
                    DifficultyLevel = "Medium",
                    Options = q.Options.Select(opt => new DataModel.OptionModel
                    {
                        Text = string.IsNullOrEmpty(opt.OptionImageUrl) 
                            ? opt.OptionText 
                            : $"{opt.OptionText}<img src='{opt.OptionImageUrl}' style='max-width:200px;display:block;margin:5px 0;'/>",
                        IsCorrect = false
                    }).ToList()
                }).ToList();

                _logger.LogInformation("=== Final Question Summary ===");
                _logger.LogInformation("Saving to database:");
                foreach (var q in questions)
                {
                    var imgCount = q.QuestionText.Split("<img").Length - 1;
                    _logger.LogInformation($"Q{q.QuestionNumber}: {q.QuestionText.Length} chars, img tags: {imgCount}");
                }
                
                var savedIds = await _apiService.SaveQuestionsAsync(questionModels, 2);
                _logger.LogInformation($"Saved {savedIds.Count} questions to DB");

                return Ok(new { Questions = questions, DiagramCount = detectedDiagrams.Count, SavedQuestionIds = savedIds });
            }
             
        
        [HttpPost]
        public IActionResult UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            using var stream = file.OpenReadStream();
            var images = _pdfToImage.ConvertPdfToImages(stream);

            var imageData = new List<object>();
            for (int i = 0; i < images.Count; i++)
            {
                var fileName = $"page_{i + 1}_{DateTime.Now:yyyyMMddHHmmss}.png";
                imageData.Add(new { FileName = fileName, Data = Convert.ToBase64String(images[i]) });
            }

            return Ok(new { Message = $"Converted {images.Count} images", Images = imageData });
        }
    }
}
