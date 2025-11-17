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

    [Authorize]
    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUserByUsernameAsync(string username)
    {
        var user = await _authService.GetUserByUsernameAsync(username);
        if (user == null) return NotFound();
        return Ok(new { user.InstituteId, user.FullName });
    }

    [HttpPost("getHash")]
    public IActionResult HashPassword([FromBody] string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return Ok(new { Hash = hash });
    }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}
