using Microsoft.AspNetCore.Mvc;
using DAL.Repository;
using DataModel;
using ExamifyAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExamifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<QuestionModel>>> GetAll()
        {
            var questions = await _questionService.GetAllQuestionsAsync();
            return Ok(questions);
        }

        [HttpGet("GetQuestionById/{id}")]
        public async Task<ActionResult<QuestionModel>> Get(int id)
        {
            var question = await _questionService.GetQuestionByIdAsync(id);
            if (question == null) return NotFound();
            return Ok(question);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] QuestionModel model)
        {
            var id = await _questionService.CreateQuestionAsync(model);
            return Ok(id);
        }

        [HttpPut]
        public async Task<ActionResult<int>> Update([FromBody] QuestionModel model)
        {
            var result = await _questionService.UpdateQuestionAsync(model);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<int>> Delete(int id)
        {
            var result = await _questionService.DeleteQuestionAsync(id);
            return Ok(result);
        }

        [HttpGet("types")]
        public async Task<ActionResult<List<QuestionTypeModel>>> GetQuestionTypes()
        {
            var types = await _questionService.GetQuestionTypesAsync();
            return Ok(types);
        }
    }
}
