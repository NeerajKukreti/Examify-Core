using DataModel;
using Examify.Attributes;
using Examify.Common.constants;
using Examify.Services;
using Examify.Extensions;
using Examify.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Examify.Controllers.Admin
{
    [Authorize(Roles = "Institute")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _QuestionService;
        private readonly IFileService _fileService;

        public QuestionController(IQuestionService QuestionService, IFileService fileService)
        {
            _QuestionService = QuestionService;
            _fileService = fileService;
        }

        // GET: Admin/Question
        public IActionResult Index()
        {
            // Do not load questions here, just return the view
            return View("Index");
        }

        // GET: Admin/Question/LoadQuestion
        [HttpGet]
        public async Task<IActionResult> LoadQuestion()
        {
            var questions = await _QuestionService.GetAllAsync();
            return Json(new { data = questions });
        }

        public IActionResult TEst()
        {
            return PartialView("_Quill");
        }

        // GET: Admin/Question/Create
        public IActionResult Create()
        {
            ViewBag.instituteId = User.GetInstituteId();
            return PartialView("_Create", new QuestionModel());
        }

        // POST: Admin/Question/Create
        [HttpPost]
        public async Task<IActionResult> Create(QuestionModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _QuestionService.CreateAsync(model);
                if (success)
                    return Json(new { success = true }); // AJAX expects JSON
            }
            // Optionally, return validation errors
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToList();
            return Json(new { success = false, errors });
        }

        // GET: Admin/Question/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.instituteId = User.GetInstituteId();

            var question = await _QuestionService.GetByIdAsync(id);

            if (question == null) return NotFound();

            return PartialView("_Create", question);
        }

        // POST: Admin/Question/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(QuestionModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _QuestionService.UpdateAsync(model);
                if (success)
                    return RedirectToAction("Index");
            }
            else
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToList();
                // Log or inspect 'errors' to see which fields are failing and why
            }
            return View("Edit", model);
        }

        // POST: Admin/Question/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _QuestionService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Uploads(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var fileName = await _fileService.SaveCompressedImage(file, "QuesionUploads");
                var fileUrl = $"/{Question.QuestionUploads}/{fileName}";
                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing image: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult DeleteUploadedImages([FromBody] List<string> fileNames)
        {
            if (fileNames == null || !fileNames.Any())
                return BadRequest("No files specified.");

            try
            {
                _fileService.DeleteFiles(fileNames, "QuesionUploads");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting images: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult Remove([FromBody] dynamic data)
        {
            try
            {
                string url = data.url;
                var fileName = url.Split('/').Last();
                _fileService.DeleteFile(fileName, "QuesionUploads");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error removing image: {ex.Message}");
            }
        }


    }
}
