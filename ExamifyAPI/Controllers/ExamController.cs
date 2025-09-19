using Microsoft.AspNetCore.Mvc;
using ExamifyAPI.Services;
using DataModel;
using DataModel.Exam;


namespace ExamifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        [HttpGet("list")]
        public IActionResult GetExamList()
        {
            try
            {
                var exams = _examService.GetActiveExams();
                return Ok(exams);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("GetExamById/{id}")]
        public IActionResult GetExamById(int id)
        {
            try
            {
                var exam = _examService.GetExamById(id);
                if (exam == null)
                    return NotFound();
                    
                return Ok(exam);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

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
    }
}