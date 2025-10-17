using Microsoft.AspNetCore.Mvc;
using ExamifyAPI.Services;
using Model.DTO;
using ExamAPI.Services;

namespace ExamifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly IAuthService _authService;

        public SubjectController(ISubjectService subjectService, IAuthService authService)
        {
            _subjectService = subjectService;
            _authService = authService;
        }

        [HttpGet("{instituteId:int}/{subjectId:int}")]
        public async Task<IActionResult> GetAllSubjects(int instituteId, int? subjectId = 0)
        {
            var subjects = await _subjectService.GetAllSubjectsAsync(instituteId, subjectId);
            if (subjects == null) return NotFound(new { Success = false, Message = "Subject not found" });
            return Ok(new { Success = true, Count = subjects?.Count() ?? 0, Data = subjects });
        }

        [HttpGet("{subjectId:int}/topics")]
        public async Task<IActionResult> GetTopicsBySubjectId(int subjectId)
        {
            var topics = await _subjectService.GetTopicsBySubjectIdAsync(subjectId);
            return Ok(new { Success = true, Data = topics });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubject([FromBody] SubjectDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Errors = ModelState });

            var userId = _authService.GetCurrentUserID();
            var newId = await _subjectService.InsertOrUpdateSubjectAsync(dto, null, userId);

            return CreatedAtAction(nameof(GetAllSubjects),
                new { instituteId = dto.InstituteId, subjectId = newId },
                new { Success = true, SubjectId = newId, Message = "Subject created successfully." });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] SubjectDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(new { Success = false, Errors = ModelState });

            var userId = _authService.GetCurrentUserID();
            var updatedId = await _subjectService.InsertOrUpdateSubjectAsync(dto, id, userId);

            return Ok(new { Success = true, SubjectId = updatedId, Message = "Subject updated successfully." });
        }

        [HttpPut("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus([FromQuery] int id)
        {
            var success = await _subjectService.ChangeStatusAsync(id);

            if (success)
                return Ok(new { Success = true, SubjectId = id, Message = "Subject status updated successfully." });
            else
                return NotFound(new { Success = false, SubjectId = id, Message = "Subject not found or update failed." });
        }
    }
}
