using Examify.Attributes;
using Examify.Services;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;

namespace Examify.Controllers.Admin
{
    [AutoLoginAuthorize("admin", "Test@123", "Admin", "Teacher")]
    public class SubjectController : Controller
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> LoadSubjects()
        {
            var instituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;
            var subjects = await _subjectService.GetAllAsync(instituteId);
            return Json(new { data = subjects });
        }

        public async Task<IActionResult> Create()
        {
            var model = new SubjectDTO
            {
                SubjectName = "",
                InstituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3,
                Topics = new List<SubjectTopicDTO>()
            };

            return PartialView("_Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SubjectDTO model)
        {
            if (ModelState.IsValid)
            {
                model.InstituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;
                model.IsActive = true;

                var success = await _subjectService.CreateAsync(model);
                if (success)
                    return Json(new { success = true, message = "Subject created successfully!" });
                else
                    return Json(new { success = false, message = "Failed to create subject. Please try again." });
            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();
            return Json(new { success = false, errors });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var instituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;
            var subject = await _subjectService.GetByIdAsync(id, instituteId);

            if (subject == null) return NotFound();

            var topics = await _subjectService.GetTopicsBySubjectIdAsync(id);

            var model = new SubjectDTO
            {
                SubjectId = subject.SubjectId,
                InstituteId = instituteId,
                SubjectName = subject.SubjectName,
                Description = subject.Description,
                Image = subject.Image,
                Topics = topics.Select(t => new SubjectTopicDTO
                {
                    TopicId = t.TopicId,
                    SubjectId = t.SubjectId,
                    TopicName = t.TopicName,
                    Description = t.Description,
                    IsActive = t.IsActive
                }).ToList()
            };

            return PartialView("_Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubjectDTO model)
        {
            if (ModelState.IsValid)
            {
                model.InstituteId = HttpContext.Session.GetInt32("InstituteId") ?? 3;

                var success = await _subjectService.UpdateAsync(model);
                if (success)
                    return Json(new { success = true, message = "Subject updated successfully!" });
                else
                    return Json(new { success = false, message = "Failed to update subject. Please try again." });
            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var success = await _subjectService.ChangeStatusAsync(id);

            if (success)
            {
                return Json(new
                {
                    success = true,
                    message = "Subject status updated successfully!"
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to update subject status. Please try again."
                });
            }
        }
    }
}
