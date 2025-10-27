using DataModel;
using Examify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

public class AuthController : Controller
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Login(UserDTO dto)
    {
        var result = await _authService.LoginAsync(dto);
        
        if (!result.Success)
        {
            ModelState.AddModelError("", result.ErrorMessage ?? "Login failed");
            return View("Index", dto);
        }

        _authService.SaveSession(result.Token!, result.RefreshToken!, result.UserId);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.UserId!),
            new Claim(ClaimTypes.Role, HttpContext.Session.GetString("UserRole") ?? "Admin")
        };
        
        var identity = new ClaimsIdentity(claims, "SessionScheme");
        var principal = new ClaimsPrincipal(identity);
        
        await HttpContext.SignInAsync("SessionScheme", principal);
        
        return RedirectToAction("Index", "Dashboard");
    }

    public async Task<IActionResult> Logout()
    {
        _authService.ClearSession();
        await HttpContext.SignOutAsync("SessionScheme");
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult UpdateTokens(string accessToken, string refreshToken)
    {
        HttpContext.Session.SetString("JWToken", accessToken);
        HttpContext.Session.SetString("RefreshToken", refreshToken);
        return Ok();
    }
}
