// Controllers/AuthController.cs
using DataModel;
using ExamAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserDTO dto)
    {
        var response = await _authService.Authenticate(dto.Username, dto.Password);
        if (response == null) return Unauthorized();
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshToken(request.RefreshToken);
        if (response == null) return Unauthorized();
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDTO dto)
    {
        var id = await _authService.Register(dto.Username, dto.Password, "Student");
        return Ok(new { UserId = id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly() => Ok("Welcome Admin!");
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}
