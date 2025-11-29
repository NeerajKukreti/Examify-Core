using Examify.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using System.Security.Claims;

namespace Examify.Controllers.Admin
{
    [Authorize]
    public class RandomizationPresetController : Controller
    {
        private readonly IRandomizationPresetService _service;

        public RandomizationPresetController(IRandomizationPresetService service)
        {
            _service = service;
        }

        private int GetInstituteId() => int.Parse(User.FindFirstValue("InstituteId") ?? "0");

        public IActionResult Index()
        {
            ViewBag.InstituteId = GetInstituteId();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var presets = await _service.GetAllPresetsAsync(GetInstituteId());
            return Json(new { data = presets });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var preset = await _service.GetPresetByIdAsync(id);
            return Json(new { success = preset != null, data = preset });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RandomizationPresetDTO preset)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data" });

            preset.InstituteId = GetInstituteId();
            preset.CreatedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            var id = await _service.CreatePresetAsync(preset);
            return Json(new { success = true, message = "Preset created successfully", data = id });
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] RandomizationPresetDTO preset)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data" });

            var result = await _service.UpdatePresetAsync(preset);
            return Json(new { success = result, message = result ? "Preset updated successfully" : "Update failed" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeletePresetAsync(id);
            return Json(new { success = result, message = result ? "Preset deleted successfully" : "Delete failed" });
        }

        [HttpGet]
        public async Task<IActionResult> Preview(int presetId)
        {
            var preview = await _service.PreviewPresetAsync(presetId, GetInstituteId());
            return Json(new { success = true, data = preview });
        }

        [HttpPost]
        public async Task<IActionResult> Execute([FromBody] PresetExecutionDTO execution)
        {
            var questionIds = await _service.ExecutePresetAsync(execution);
            return Json(new { success = true, message = $"{questionIds.Count} questions added", data = questionIds });
        }
    }
}
