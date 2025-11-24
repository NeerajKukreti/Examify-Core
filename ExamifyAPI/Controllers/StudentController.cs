using ExamAPI.Services;
using ExamifyAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IExamService _examService;
    private readonly IAuthService _authService;
    public StudentController(IStudentService studentService, IAuthService authService, IExamService examService)
    {
        _studentService = studentService;
        _authService = authService;
        _examService = examService;
    }

    [HttpGet("{instituteId:int}/{studentId:int}")]
    public async Task<IActionResult> GetAllStudents(int instituteId, int? studentId = 0)
    {
        instituteId = _authService.GetCurrentInstituteId();
        var students = await _studentService.GetAllStudentsAsync(instituteId, studentId);
        if (students == null) return NotFound(new { Success = false, Message = "Student not found" });
        return Ok(new { Success = true, Count = students?.Count() ?? 0, Data = students });
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent([FromBody] StudentDTO dto)
    {
        if (dto.StudentId == 0)
        {
            // create student → require password
            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required for new students.");
        }

        if (!ModelState.IsValid) return BadRequest(new { Success = false, Errors = ModelState });

        var createdBy = _authService.GetCurrentUserID(); // logged-in user ID
        var newId = await _studentService.InsertOrUpdateStudentAsync(dto, null, createdBy, null);

        return CreatedAtAction(nameof(GetAllStudents),
            new { instituteId = dto.InstituteId, studentId = newId },
            new { Success = true, StudentId = newId, Message = "Student created successfully." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(new { Success = false, Errors = ModelState });

        var existingStudent = await _studentService.GetAllStudentsAsync(dto.InstituteId, id);
        var student = existingStudent?.FirstOrDefault();
        if (student == null) return NotFound(new { Success = false, Message = "Student not found" });

        dto.UserId = student.UserId;
        var modifiedBy = _authService.GetCurrentUserID(); // logged-in user ID
        var updatedId = await _studentService.InsertOrUpdateStudentAsync(dto, id, null, modifiedBy);

        return Ok(new { Success = true, StudentId = updatedId, Message = "Student updated successfully." });
    }

    [HttpPut("ChangeStatus")]
    public async Task<IActionResult> ChangeStatus([FromQuery] int id)
    {
        // Toggle status and get success
        var success = await _studentService.ChangeStatus(id);

        if (success)
        {
            return Ok(new { Success = true, StudentId = id, Message = "Student updated successfully." });
        }
        else
        {
            return NotFound(new { Success = false, StudentId = id, Message = "Student not found or update failed." });
        }
    }

    [HttpGet("Exam/list")]
    public async Task<IActionResult> GetExamList()
    {
        try
        {
            var userid = _authService.GetCurrentUserID();
            var exams = await _studentService.GetAllExamsAsync();
            var userExams = await _examService.GetUserExamsAsync(new List<long>(){userid});

            return Ok(new
            {
                Success = true,
                Count = exams?.Count() ?? 0,
                Data = exams,
                UserExams = userExams
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
        }
    }
}
