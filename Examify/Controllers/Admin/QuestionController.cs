using DataModel;
using Examify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OnlineExam.Controllers.Admin
{
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
            return PartialView("_Create");
        }

        // POST: Admin/Question/Create
        [HttpPost]
        public async Task<IActionResult> Create(QuestionModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _QuestionService.CreateAsync(model);
                if (success)
                    return RedirectToAction("Index");
            }
            return View("Create", model);
        }

        // GET: Admin/Question/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var question = new QuestionModel();//await _QuestionService.GetByIdAsync(id);
            if (question == null) return NotFound();
            return View("Edit", question);
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

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the public URL
            var fileUrl = $"/uploads/{fileName}";
            return Ok(new { url = fileUrl });
        }


    }
}
