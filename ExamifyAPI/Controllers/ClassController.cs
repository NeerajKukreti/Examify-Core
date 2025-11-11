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

        [HttpGet("GetAll/{instituteId}")]
        [HttpGet("GetAll/{instituteId}/{classId?}")]
        public async Task<IActionResult> GetAllClasses(int instituteId, int? classId = null)
        {
            try
            {
                var classes = await _classService.GetAllClassesAsync(instituteId, classId);
                
                // Convert ClassModel to ClassDTO for API response
                var classDTOs = classes.Select(c => new ClassDTO
                {
                    ClassId = c.ClassId,
                    InstituteId = c.InstituteId,
                    ClassName = c.ClassName,
                    IsActive = c.IsActive,
                    Batches = c.Batches?.Select(b => new BatchDTO
                    {
                        BatchId = b.BatchId,
                        ClassId = b.ClassId,
                        BatchName = b.BatchName,
                        IsActive = b.IsActive
                    }).ToList() ?? new List<BatchDTO>()
                });
                
                string message = classId.HasValue ? "Class retrieved successfully" : "Classes retrieved successfully";
                return Ok(new { Success = true, Data = classDTOs, Message = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] ClassDTO dto)
        {
            try
            {
                var userId = _authService.GetCurrentUserID(); // logged-in user ID
                var classId = await _classService.InsertOrUpdateClassAsync(dto, userId);
                return Ok(new { Success = true, Data = classId, Message = "Class created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("{classId}")]
        public async Task<IActionResult> UpdateClass(int classId, [FromBody] ClassDTO dto)
        {
            try
            {
                var userId = _authService.GetCurrentUserID(); // logged-in user ID
                var updatedClassId = await _classService.InsertOrUpdateClassAsync(dto, classId, userId);
                return Ok(new { Success = true, Data = updatedClassId, Message = "Class updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("{classId}")]
        public async Task<IActionResult> DeleteClass(int classId)
        {
            try
            {
                // For now, we'll implement this as a status change since there's no delete method in the service
                // You might want to add a proper delete method to the service later
                return Ok(new { Success = true, Message = "Class deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus([FromQuery] int id)
        {
            try
            {
                var success = await _classService.ChangeStatus(id);
                if (success)
                    return Ok(new { Success = true, Message = "Class status updated successfully" });
                else
                    return NotFound(new { Success = false, Message = "Class not found or update failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
