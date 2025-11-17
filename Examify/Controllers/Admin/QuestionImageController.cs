using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.RegularExpressions;

namespace Examify.Controllers.Admin
{
    public class QuestionImageController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public QuestionImageController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImages(List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
            {
                TempData["Error"] = "Please select at least one image";
                return RedirectToAction("Index");
            }

            try
            {
                var uploadFolder = Path.Combine(_environment.WebRootPath, "QuestionImages");
                Directory.CreateDirectory(uploadFolder);

                var uploadedFiles = new List<string>();

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        var ext = Path.GetExtension(image.FileName).ToLower();
                        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                            continue;

                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(uploadFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        uploadedFiles.Add($"/QuestionImages/{fileName}");
                    }
                }

                var extractedQuestions = ExtractWithOCR(uploadedFiles);
                
                HttpContext.Session.SetString("ExtractedQuestions", 
                    System.Text.Json.JsonSerializer.Serialize(extractedQuestions));
                HttpContext.Session.SetString("UploadedImages", string.Join(",", uploadedFiles));
                
                TempData["Success"] = $"Uploaded {uploadedFiles.Count} images. Extracted {extractedQuestions.Count} questions.";
                
                return RedirectToAction("CreateQuestions");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult CreateQuestions()
        {
            var imagePaths = HttpContext.Session.GetString("UploadedImages")?.Split(',').ToList() ?? new List<string>();
            var extractedJson = HttpContext.Session.GetString("ExtractedQuestions");
            var extractedQuestions = string.IsNullOrEmpty(extractedJson) 
                ? new List<QuestionImageModel>() 
                : System.Text.Json.JsonSerializer.Deserialize<List<QuestionImageModel>>(extractedJson);
            
            if (imagePaths.Count == 0)
            {
                TempData["Error"] = "No images found";
                return RedirectToAction("Index");
            }
            
            ViewBag.ImagePaths = imagePaths;
            ViewBag.ExtractedQuestions = extractedQuestions;
            return View();
        }

        [HttpPost]
        public IActionResult SaveQuestions(List<QuestionImageModel> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                TempData["Error"] = "No questions to save";
                return RedirectToAction("Index");
            }

            // TODO: Save to database

            HttpContext.Session.Remove("UploadedImages");
            HttpContext.Session.Remove("ExtractedQuestions");
            TempData["Success"] = $"Saved {questions.Count} questions";
            return RedirectToAction("Index");
        }

        private List<QuestionImageModel> ExtractWithOCR(List<string> imagePaths)
        {
            var questions = new List<QuestionImageModel>();
            var pythonScript = Path.Combine(_environment.ContentRootPath, "paddle_ocr.py");
            
            if (!System.IO.File.Exists(pythonScript))
                return questions;

            foreach (var imagePath in imagePaths)
            {
                try
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/').Replace('/', '\\'));
                    var text = RunPythonOCR(pythonScript, fullPath);
                    var parsedQuestions = ParseMultipleQuestions(text, imagePath);
                    questions.AddRange(parsedQuestions);
                }
                catch { }
            }

            return questions;
        }

        private string RunPythonOCR(string scriptPath, string imagePath)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\" \"{imagePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,

               
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = System.Diagnostics.Process.Start(psi);
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
                throw new Exception($"Python error: {error}");

            return output;

        }

        private bool IsHindi(string text)
        {
            return text.Any(c => c >= '\u0900' && c <= '\u097F');
        }

        private string NormalizeOptions(string input)
        {
            string t = input;

            // Clean HTML entities
            t = t.Replace("&quot;", "\"");
            t = t.Replace("&#39;", "'");
            t = t.Replace("&amp;", "&");
            
            // Fix double parentheses from OCR
            t = t.Replace("((", "(");
            
            // fix broken option markers
            t = Regex.Replace(t, @"\(\s*1\s*\)", "(1)");
            t = Regex.Replace(t, @"\(\s*2\s*\)", "(2)");
            t = Regex.Replace(t, @"\(\s*3\s*\)", "(3)");
            t = Regex.Replace(t, @"\(\s*4\s*\)", "(4)");

        

            return t;
        }

        private List<QuestionImageModel> ParseMultipleQuestions(string text, string imagePath)
        {
            var questions = new List<QuestionImageModel>();
            text = NormalizeOptions(text);

            // Split by question numbers (1., 2., 3., etc.) at start of line
            var blocks = Regex.Split(text, @"(?=^\d+\.\s)", RegexOptions.Multiline)
                              .Where(b => !string.IsNullOrWhiteSpace(b) && b.Trim().Length > 20)
                              .ToList();

            foreach (var block in blocks)
            {
                if (!block.Contains("(1)")) continue;

                // Split by option markers
                var parts = Regex.Split(block, @"\((1|2|3|4)\)");
                if (parts.Length < 9) continue;

                var questionText = parts[0].Trim();
                if (questionText.Length < 10) continue;

                var opt1 = parts[2].Trim();
                var opt2 = parts[4].Trim();
                var opt3 = parts[6].Trim();
                var opt4 = parts[8].Trim();

                questions.Add(new QuestionImageModel
                {
                    ImagePath = imagePath,
                    ExtractedText = block.Trim(),
                    QuestionText = questionText,
                    Option1 = opt1,
                    Option2 = opt2,
                    Option3 = opt3,
                    Option4 = opt4
                });
            }

            return questions;
        }
    }

    public class QuestionImageModel
    {
        public string ImagePath { get; set; }
        public string QuestionText { get; set; }
        public string ExtractedText { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public int CorrectOption { get; set; }
    }
}
