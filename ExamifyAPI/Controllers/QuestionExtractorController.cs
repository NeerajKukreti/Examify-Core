using DataModel;
using ExamifyAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamifyAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionExtractorController : ControllerBase
    {
        private readonly IQuestionExtractorService _service;

        public QuestionExtractorController(IQuestionExtractorService service)
        {
            _service = service;
        }

        [HttpPost("SaveQuestion")]
        public async Task<IActionResult> SaveQuestion([FromBody] QuestionModel model, [FromQuery] int instituteId)
        {
            var questionId = await _service.SaveExtractedQuestionAsync(model, instituteId);
            return Ok(new { QuestionId = questionId });
        }

        [HttpPost("SaveQuestions")]
        public async Task<IActionResult> SaveQuestions([FromBody] List<QuestionModel> questions, [FromQuery] int instituteId)
        {
            var questionIds = await _service.SaveExtractedQuestionsAsync(questions, instituteId);
            return Ok(new { QuestionIds = questionIds, Count = questionIds.Count });
        }
    }
}
