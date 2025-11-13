using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataModel;
using Examify.Services;
using Examify.Extensions;
using Model.DTO;

namespace Examify.Controllers.Admin
{
    [Authorize(Roles = "Institute")]
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

        public async Task<IActionResult> ConfigureQuestions(int id)
        {
            var exam = await _examService.GetByIdAsync(id);
            if (exam == null) return NotFound();

            ViewBag.ExamId = id;
            ViewBag.ExamName = exam.ExamName;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableQuestions(int examId)
        {
            var instituteId = User.GetInstituteId() ?? 3;
            var questions = await _examService.GetAvailableQuestionsAsync(examId, instituteId);
            return Json(new { data = questions });
        }

        [HttpGet]
        public async Task<IActionResult> GetExamQuestions(int examId)
        {
            var questions = await _examService.GetExamQuestionsAsync(examId);
            return Json(new { data = questions });
        }

        [HttpPost]
        public async Task<IActionResult> SaveExamQuestions([FromBody] ExamQuestionConfigDTO config)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();
                return Json(new { success = false, errors });
            }

            var success = await _examService.SaveExamQuestionsAsync(config);
            if (success)
                return Json(new { success = true, message = "Questions configured successfully!" });
            else
                return Json(new { success = false, message = "Failed to configure questions." });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveExamQuestion(int examId, int questionId)
        {
            var success = await _examService.RemoveExamQuestionAsync(examId, questionId);
            if (success)
                return Json(new { success = true, message = "Question removed successfully!" });
            else
                return Json(new { success = false, message = "Failed to remove question." });
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _examService.GetStatsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}