using DataModel;
using ExamAPI.Services;
using ExamifyAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamifyAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionExtractorController : ControllerBase
    {
        private readonly IQuestionExtractorService _service;
        private readonly IAuthService _authService;

        public QuestionExtractorController(IQuestionExtractorService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        [HttpPost("SaveQuestion")]
        public async Task<IActionResult> SaveQuestion([FromBody] QuestionModel model, [FromQuery] int? instituteId = null)
        {
            var currentInstituteId = _authService.GetCurrentInstituteId();
            var targetInstituteId = currentInstituteId;
            
            if (currentInstituteId == 0 && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")))
            {
                targetInstituteId = instituteId ?? 0;
            }
            else if (currentInstituteId == 0)
            {
                return Forbid();
            }

            var questionId = await _service.SaveExtractedQuestionAsync(model, targetInstituteId);
            return Ok(new { QuestionId = questionId });
        }

        [HttpPost("SaveQuestions")]
        public async Task<IActionResult> SaveQuestions([FromBody] List<QuestionModel> questions, [FromQuery] int? instituteId = null)
        {
            var currentInstituteId = _authService.GetCurrentInstituteId();
            var targetInstituteId = currentInstituteId;
            
            if (currentInstituteId == 0 && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")))
            {
                targetInstituteId = instituteId ?? 0;
            }
            else if (currentInstituteId == 0)
            {
                return Forbid();
            }

            var questionIds = await _service.SaveExtractedQuestionsAsync(questions, targetInstituteId);
            return Ok(new { QuestionIds = questionIds, Count = questionIds.Count });
        }
    }
}
