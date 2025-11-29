using ExamifyAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using DataModel.Common;

namespace ExamifyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RandomizationPresetController : ControllerBase
    {
        private readonly IRandomizationPresetService _service;

        public RandomizationPresetController(IRandomizationPresetService service)
        {
            _service = service;
        }

        [HttpGet("{instituteId}")]
        public async Task<IActionResult> GetAll(int instituteId)
        {
            var presets = await _service.GetAllPresetsAsync(instituteId);
            return Ok(new ApiResponse<IEnumerable<RandomizationPresetDTO>> 
            { 
                Success = true, 
                Data = presets 
            });
        }

        [HttpGet("detail/{presetId}")]
        public async Task<IActionResult> GetById(int presetId)
        {
            var preset = await _service.GetPresetByIdAsync(presetId);
            return Ok(new ApiResponse<RandomizationPresetDTO?> 
            { 
                Success = preset != null, 
                Data = preset 
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RandomizationPresetDTO preset)
        {
            var id = await _service.CreatePresetAsync(preset);
            return Ok(new ApiResponse<int> 
            { 
                Success = true, 
                Message = "Preset created", 
                Data = id 
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] RandomizationPresetDTO preset)
        {
            var result = await _service.UpdatePresetAsync(preset);
            return Ok(new ApiResponse<bool> 
            { 
                Success = result, 
                Message = result ? "Preset updated" : "Update failed" 
            });
        }

        [HttpDelete("{presetId}")]
        public async Task<IActionResult> Delete(int presetId)
        {
            var result = await _service.DeletePresetAsync(presetId);
            return Ok(new ApiResponse<bool> 
            { 
                Success = result, 
                Message = result ? "Preset deleted" : "Delete failed" 
            });
        }

        [HttpGet("preview/{presetId}/{instituteId}")]
        public async Task<IActionResult> Preview(int presetId, int instituteId)
        {
            var preview = await _service.PreviewPresetAsync(presetId, instituteId);
            return Ok(new ApiResponse<PresetPreviewDTO> 
            { 
                Success = true, 
                Data = preview 
            });
        }

        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] PresetExecutionDTO execution)
        {
            var questionIds = await _service.ExecutePresetAsync(execution);
            return Ok(new ApiResponse<List<int>> 
            { 
                Success = true, 
                Message = $"{questionIds.Count} questions added", 
                Data = questionIds 
            });
        }
    }
}
