using DataModel;
using Examify.Services;
using Microsoft.AspNetCore.Mvc;

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
        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Logout()
    {
        _authService.ClearSession();
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
