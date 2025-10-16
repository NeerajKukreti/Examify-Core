using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DataModel;
using Examify.Common;
using Examify.Helpers;
using Examify.Services;
using Examify.Attributes;
using Model.DTO;

namespace Examify.Controllers.Admin
{
    [AutoLoginAuthorize("admin", "Test@123", "Admin", "Teacher")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> LoadExams()
        {
            var exams = await _examService.GetAllAsync();
            return Json(new { data = exams });
        }

        public async Task<IActionResult> Create()
        {
            var model = new ExamDTO
            {
                ExamName = "",
                DurationMinutes = 60,
                TotalQuestions = 0
            };

            return PartialView("_Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExamDTO model)
        {
            if (ModelState.IsValid)
            {
                model.IsActive = true;
                
                var success = await _examService.CreateAsync(model);
                if (success)
                    return Json(new { success = true, message = "Exam created successfully!" });
                else
                    return Json(new { success = false, message = "Failed to create exam. Please try again." });
            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();
            return Json(new { success = false, errors });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var exam = await _examService.GetByIdAsync(id);

            if (exam == null) return NotFound();

            var model = new ExamDTO
            {
                ExamId = exam.ExamId,
                ExamName = exam.ExamName,
                Description = exam.Description,
                Image = exam.Image,
                DurationMinutes = exam.DurationMinutes,
                TotalQuestions = exam.TotalQuestions,
                Instructions = exam.Instructions,
                ExamType = exam.ExamType,
                CutOffPercentage = exam.CutOffPercentage
            };

            return PartialView("_Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ExamDTO model)
        {
            if (ModelState.IsValid)
            {
                var success = await _examService.UpdateAsync(model);
                if (success)
                    return Json(new { success = true, message = "Exam updated successfully!" });
                else
                    return Json(new { success = false, message = "Failed to update exam. Please try again." });
            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            try
            {
                var success = await _examService.ChangeStatusAsync(id);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Exam status updated successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to update exam status. Please try again."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while updating exam status."
                });
            }
        }
    }
}