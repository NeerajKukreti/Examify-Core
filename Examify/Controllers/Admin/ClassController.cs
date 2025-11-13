using Examify.Attributes;
using Examify.Services;
using Examify.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;

namespace Examify.Controllers.Admin
{
    [Authorize(Roles = "Institute")]
    public class ClassController : Controller
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        // GET: Admin/Class
        public IActionResult Index()
        {
            return View("Index");
        }

        // GET: Admin/Class/LoadClasses
        [HttpGet]
        [Cached(5, "classes")]
        public async Task<IActionResult> LoadClasses()
        {
            var instituteId = User.GetInstituteId() ?? 3;

            var classes = await _classService.GetAllAsync(instituteId);
            return Json(new { data = classes });
        }

        // GET: Admin/Class/Create
        public async Task<IActionResult> Create()
        {
            var model = new ClassDTO
            {
                ClassName = "",
                InstituteId = User.GetInstituteId() ?? 3,
                IsActive = true,
                Batches = new List<BatchDTO>
                {
                    new BatchDTO { BatchId = 0, ClassId = 0, BatchName = "", IsActive = true }
                }
            };

            return PartialView("_Create", model);
        }

        // POST: Admin/Class/Create
        [HttpPost]
        [InvalidateCache("classes")]
        public async Task<IActionResult> Create(ClassDTO model)
        {
            if (ModelState.IsValid)
            {
                model.InstituteId = User.GetInstituteId() ?? 3;
                model.IsActive = true;

                // Ensure we have at least one batch
                if (model.Batches == null || !model.Batches.Any())
                {
                    return Json(new { success = false, message = "At least one batch is required for a class." });
                }

                var success = await _classService.CreateAsync(model);
                if (success)
                    return Json(new { success = true, message = "Class created successfully!" });
                else
                    return Json(new { success = false, message = "Failed to create class. Please try again." });
            }

            // Return validation errors
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();
            return Json(new { success = false, errors });
        }

        // GET: Admin/Class/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var insId = User.GetInstituteId() ?? 3;
            var classDto = await _classService.GetByIdAsync(insId, id);

            if (classDto == null) return NotFound();

            return PartialView("_Create", classDto);
        }

        // POST: Admin/Class/Edit
        [HttpPost]
        [InvalidateCache("classes")]
        public async Task<IActionResult> Edit(ClassDTO model)
        {
            if (ModelState.IsValid)
            {
                model.InstituteId = User.GetInstituteId() ?? 3;

                // Ensure we have at least one batch
                if (model.Batches == null || !model.Batches.Any())
                {
                    return Json(new { success = false, message = "At least one batch is required for a class." });
                }

                var success = await _classService.UpdateAsync(model);
                if (success)
                    return Json(new { success = true, message = "Class updated successfully!" });
                else
                    return Json(new { success = false, message = "Failed to update class. Please try again." });
            }

            // Return validation errors
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();
            return Json(new { success = false, errors });
        }

        // POST: Admin/Class/Delete/{id}
        [HttpPost]
        [InvalidateCache("classes")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _classService.DeleteAsync(id);
            if (success)
                return Json(new { success = true, message = "Class deleted successfully!" });
            else
                return Json(new { success = false, message = "Failed to delete class. Please try again." });
        }

        [HttpPost]
        [InvalidateCache("classes")]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var success = await _classService.ChangeStatusAsync(id);

            if (success)
            {
                return Json(new
                {
                    success = true,
                    message = "Class status updated successfully!"
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to update class status. Please try again."
                });
            }
        }
    }
}