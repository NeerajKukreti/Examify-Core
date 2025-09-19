using Microsoft.AspNetCore.Mvc;
using ExamifyAPI.Services;
using DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExamifyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _service;
        public SubjectController(ISubjectService service)
        {
            _service = service;
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<SubjectModel>>> GetSubjects([FromQuery] int instituteId, [FromQuery] int? subjectId = null)
        {
            var subjects = await _service.GetSubjectsAsync(instituteId, subjectId);
            return Ok(subjects);
        }

        [HttpGet("topics")]
        public async Task<ActionResult<List<SubjectTopicModel>>> GetSubjectTopics([FromQuery] int instituteId, [FromQuery] int? subjectId = null, [FromQuery] int? topicId = null)
        {
            var topics = await _service.GetSubjectTopicsAsync(instituteId, subjectId, topicId);
            return Ok(topics);
        }
    }
}
