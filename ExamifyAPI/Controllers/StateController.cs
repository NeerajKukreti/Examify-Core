using ExamifyAPI.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class StateController : ControllerBase
{
    private readonly IStateService _stateService;

    public StateController(IStateService stateService)
    {
        _stateService = stateService;
    }

    /// <summary>
    /// Get all states
    /// </summary>
    /// <returns>List of all states</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllStates()
    {
        try
        {
            var states = await _stateService.GetAllStatesAsync();
            return Ok(new { 
                Success = true, 
                Count = states?.Count() ?? 0, 
                Data = states 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Success = false, 
                Message = "An error occurred while retrieving states.",
                Error = ex.Message 
            });
        }
    }
}