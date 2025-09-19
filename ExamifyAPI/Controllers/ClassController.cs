using ExamAPI.Services;
using ExamifyAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;

namespace ExamifyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly IAuthService _authService;

        public ClassController(IClassService classService, IAuthService authService)
        {
            _classService = classService;
            _authService = authService;
        }

        [HttpGet("{instituteId}")]
        public async Task<IActionResult> GetAllClasses(int instituteId)
        {
            var classes = await _classService.GetAllClassesAsync(instituteId);
            return Ok(classes);
        }

        [HttpGet("details/{classId}")]
        public async Task<IActionResult> GetClassById(int classId)
        {
            var classObj = await _classService.GetClassByIdAsync(classId);
            if (classObj == null) return NotFound();
            return Ok(classObj);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] ClassDTO dto)
        {
            var userId = _authService.GetCurrentUserID(); // logged-in user ID
            var classId = await _classService.InsertOrUpdateClassAsync(dto, createdBy: userId);
            return Ok(new { ClassId = classId });
        }

        [HttpPut("{classId}")]
        public async Task<IActionResult> UpdateClass(int classId, [FromBody] ClassDTO dto)
        {
            var userId = _authService.GetCurrentUserID(); // logged-in user ID
            var updatedClassId = await _classService.InsertOrUpdateClassAsync(dto, classId, modifiedBy: userId);
            return Ok(new { ClassId = updatedClassId });
        }
    }

}
