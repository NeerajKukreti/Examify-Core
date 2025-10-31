using DataModel;
using Examify.Attributes;
using Examify.Common.constants;
using Examify.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Examify.Controllers.Admin
{
    //[AutoLoginAuthorize("admin", "Test@123", "Admin", "Teacher")] // Auto-login with credentials and restrict to Admin/Teacher roles
    [Authorize(Roles = "Institute")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _QuestionService;
        private readonly IWebHostEnvironment _env;

        public QuestionController(IQuestionService QuestionService, IWebHostEnvironment env)
        {
            _QuestionService = QuestionService;
            _env = env;
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
            ViewBag.instituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;
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
            ViewBag.instituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;

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

            var uploadsPath = Path.Combine(_env.WebRootPath, "QuesionUploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the public URL
            var fileUrl = $"/{Question.QuestionUploads}/{fileName}";
            return Ok(new { url = fileUrl });
        }


    }
}
