using ExamAPI.Services;
using ExamifyAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IAuthService _authService;
    public StudentController(IStudentService studentService, IAuthService authService)
    {
        _studentService = studentService;
        _authService = authService;
    }

    [HttpGet("list/{instituteId:int}")]
    public async Task<IActionResult> GetAllStudents(int instituteId)
    {
        var students = await _studentService.GetAllStudentsAsync(instituteId);
        return Ok(new { Success = true, Count = students?.Count() ?? 0, Data = students });
    }

    [HttpGet("{instituteId:int}/{studentId:int}")]
    public async Task<IActionResult> GetStudentById(int instituteId, int studentId)
    {
        var student = await _studentService.GetStudentByIdAsync(instituteId, studentId);
        if (student == null) return NotFound(new { Success = false, Message = "Student not found" });
        return Ok(new { Success = true, Message = "Student retrieved successfully", Data = student });
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] StudentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(new { Success = false, Errors = ModelState });
        var createdBy = _authService.GetCurrentUserID(); // logged-in user ID
        var newId = await _studentService.InsertOrUpdateStudentAsync(dto, null, createdBy, null);

        return CreatedAtAction(nameof(GetStudentById),
            new { instituteId = dto.InstituteId, studentId = newId },
            new { Success = true, StudentId = newId, Message = "Student created successfully." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(new { Success = false, Errors = ModelState });

        var existingStudent = await _studentService.GetStudentByIdAsync(dto.InstituteId, id);
        if (existingStudent == null) return NotFound(new { Success = false, Message = "Student not found" });

        dto.UserId = existingStudent.UserId;
        var modifiedBy = _authService.GetCurrentUserID(); // logged-in user ID
        var updatedId = await _studentService.InsertOrUpdateStudentAsync(dto, id, null, modifiedBy);

        return Ok(new { Success = true, StudentId = updatedId, Message = "Student updated successfully." });
    }
}
