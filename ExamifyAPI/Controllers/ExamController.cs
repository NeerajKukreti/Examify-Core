using Microsoft.AspNetCore.Mvc;
using ExamifyAPI.Services;
using DataModel;
using DataModel.Exam;
using Model.DTO;
using ExamAPI.Services;

namespace ExamifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly IAuthService _authService;

        public ExamController(IExamService examService, IAuthService authService)
        {
            _examService = examService;
            _authService = authService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetExamList()
        {
            try
            {
                var exams = await _examService.GetAllExamsAsync();
                return Ok(new { Success = true, Count = exams?.Count() ?? 0, Data = exams });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("{examId:int}")]
        public async Task<IActionResult> GetExamById(int examId)
        {
            try
            {
                var exam = await _examService.GetExamByIdAsync(examId);
                if (exam == null)
                    return BadRequest(new { Success = false, Message = "User has already taken the exam" });
                    
                return Ok(new { Success = true, Data = exam });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateExam([FromBody] ExamDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Errors = ModelState });

            try
            {
                var createdBy = _authService.GetCurrentUserID();
                var newId = await _examService.InsertOrUpdateExamAsync(dto, null, createdBy);

                return CreatedAtAction(nameof(GetExamById),
                    new { examId = newId },
                    new { Success = true, ExamId = newId, Message = "Exam created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateExam(int id, [FromBody] ExamDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Errors = ModelState });

            try
            {
                var existingExam = await _examService.GetExamByIdAsync(id);
                if (existingExam == null)
                    return NotFound(new { Success = false, Message = "Exam not found" });

                var modifiedBy = _authService.GetCurrentUserID();
                var updatedId = await _examService.InsertOrUpdateExamAsync(dto, id, modifiedBy);

                return Ok(new { Success = true, ExamId = updatedId, Message = "Exam updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpPut("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus([FromQuery] int id)
        {
            try
            {
                var success = await _examService.ChangeStatusAsync(id);

                if (success)
                    return Ok(new { Success = true, ExamId = id, Message = "Exam status updated successfully." });
                else
                    return NotFound(new { Success = false, ExamId = id, Message = "Exam not found or update failed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost("{id}/publish")]
        public async Task<IActionResult> PublishExam(int id)
        {
            try
            {
                var success = await _examService.PublishExamAsync(id);

                if (success)
                    return Ok(new { Success = true, Message = "Exam publish status updated successfully." });
                else
                    return NotFound(new { Success = false, Message = "Exam not found or update failed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

 

        #region Exam Question Configuration
        [HttpGet("{examId}/available-questions")]
        public async Task<IActionResult> GetAvailableQuestions(int examId, [FromQuery] int instituteId)
        {
            try
            {
                var questions = await _examService.GetAvailableQuestionsAsync(examId, instituteId);
                return Ok(new { Success = true, Data = questions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("{examId}/questions")]
        public async Task<IActionResult> GetExamQuestions(int examId)
        {
            try
            {
                var questions = await _examService.GetExamQuestionsAsync(examId);
                return Ok(new { Success = true, Data = questions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost("{examId}/questions")]
        public async Task<IActionResult> SaveExamQuestions(int examId, [FromBody] ExamQuestionConfigDTO config)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Errors = ModelState });

            try
            {
                var success = await _examService.SaveExamQuestionsAsync(config);
                if (success)
                    return Ok(new { Success = true, Message = "Questions configured successfully" });
                else
                    return BadRequest(new { Success = false, Message = "Failed to configure questions" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpDelete("{examId}/questions/{questionId}")]
        public async Task<IActionResult> RemoveExamQuestion(int examId, int questionId)
        {
            try
            {
                var success = await _examService.RemoveExamQuestionAsync(examId, questionId);
                if (success)
                    return Ok(new { Success = true, Message = "Question removed successfully" });
                else
                    return NotFound(new { Success = false, Message = "Question not found in exam" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }
        #endregion

        #region Exam Session
        [HttpGet("{examId}/sessionquestions")]
        public IActionResult GetExamSessionQuestions(int userId, int examId)
        {
            try
            {
                var examQuestions = _examService.GetExamSessionQuestions(userId, examId);
                if (examQuestions == null)
                    return NotFound();
                    
                return Ok(examQuestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("{examId}/submit")]
        public IActionResult SubmitExam(int examId, [FromBody] ExamSubmissionModel submission)
        {
            try
            {
                submission.ExamId = examId;
                // Map ordering and pairing items for each response
                foreach (var response in submission.Responses)
                {
                    // If ordering, ensure OrderedItems is not null
                    if (response.OrderedItems == null)
                        response.OrderedItems = new List<ExamResponseOrderModel>();
                    // If pairing, ensure PairedItems is not null
                    if (response.PairedItems == null)
                        response.PairedItems = new List<ExamResponsePairModel>();
                }
                var sessionId = _examService.SubmitExamResponses(submission);
                
                if (sessionId > 0)
                {
                    return Ok(new { Success = true, SessionId = sessionId, Message = "Exam submitted successfully" });
                }
                else
                {
                    return Ok(new { Success = false, Message = "Failed to submit exam" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("result/{sessionId}")]
        public IActionResult GetExamResult(int sessionId)
        {
            try
            {
                var result = _examService.GetExamResult(sessionId);
                if (result == null)
                    return NotFound("Exam result not found");
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("violation")]
        public IActionResult LogViolation([FromBody] ViolationModel violation)
        {
            try
            {
                // Capture client IP address
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                violation.IpAddress = ipAddress;
                
                // Log violation to console/file for now
                var logMessage = $"[VIOLATION] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - IP: {ipAddress}, Type: {violation.Type}, Description: {violation.Description}, UserAgent: {violation.UserAgent}";
                Console.WriteLine(logMessage);
                
                // TODO: Store in database if needed
                // _examService.LogViolation(violation);
                
                return Ok(new { Success = true, Message = "Violation logged successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error logging violation: {ex.Message}");
            }
        }
        #endregion

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserExams(long userId)
        {
            try
            {
                var exams = await _examService.GetUserExamsAsync(new List<long> { userId });
                return Ok(new { Success = true, Data = exams });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost("users/exams")]
        public async Task<IActionResult> GetUsersExams([FromBody] List<long> userIds)
        {
            try
            {
                var exams = await _examService.GetUserExamsAsync(userIds);
                return Ok(new { Success = true, Data = exams });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _examService.GetStatsAsync();
                return Ok(new { Success = true, Data = stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("instructions/{instituteId}")]
        public async Task<IActionResult> GetInstructions(int instituteId)
        {
            try
            {
                var instructions = await _examService.GetInstructionsAsync(instituteId);
                return Ok(new { Success = true, Data = instructions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost("instructions")]
        public async Task<IActionResult> UpsertInstruction([FromBody] ExamInstructionModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Success = false, Errors = ModelState });

            try
            {
                var id = await _examService.UpsertInstructionAsync(model);
                return Ok(new { Success = true, InstructionId = id, Message = "Instruction saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }
    }
}