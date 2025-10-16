using ExamAPI.Services;
using ExamifyAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;

namespace ExamifyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatchController : ControllerBase
    {
        private readonly IBatchService _batchService;
        private readonly IAuthService _authService;

        public BatchController(IBatchService batchService, IAuthService authService)
        {
            _batchService = batchService;
            _authService = authService;
        }

        [HttpGet("{batchId}")]
        public async Task<IActionResult> GetBatchById(int batchId)
        {
            var batch = await _batchService.GetBatchByIdAsync(batchId);
            if (batch == null) return NotFound();
            return Ok(batch);
        }

        [HttpGet("ByClass/{classId}")]
        public async Task<IActionResult> GetBatchesByClassId(int classId)
        {
            var batches = await _batchService.GetBatchesByClassIdAsync(classId);
            return Ok(new { Success = true, Data = batches, Message = "Batches retrieved successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> CreateBatch([FromBody] BatchDTO dto)
        {
            var userId = _authService.GetCurrentUserID(); // logged-in user ID
            var batchId = await _batchService.InsertOrUpdateBatchAsync(dto, createdBy: userId);
            return Ok(new { BatchId = batchId });
        }

        [HttpPut("{batchId}")]
        public async Task<IActionResult> UpdateBatch(int batchId, [FromBody] BatchDTO dto)
        {
            var userId = _authService.GetCurrentUserID(); // logged-in user ID
            var updatedBatchId = await _batchService.InsertOrUpdateBatchAsync(dto, batchId, modifiedBy: userId);
            return Ok(new { BatchId = updatedBatchId });
        }
    }
}
