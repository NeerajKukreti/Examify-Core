using DataModel;
using Examify.Services;
using Examify.Common;
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

        _authService.SaveTokensInSession(result.Token!, result.RefreshToken!);
        
        var tokenData = JwtHelper.ParseToken(result.Token!);
        await SetUserClaims(tokenData, result.InstituteId, result.FullName);
        
        return RedirectToAction("Index", "Dashboard");
    }

    public async Task<IActionResult> Logout()
    {
        _authService.ClearSession();
        await HttpContext.SignOutAsync("SessionScheme");
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTokens(string accessToken, string refreshToken)
    {
        _authService.SaveTokensInSession(accessToken, refreshToken);
        
        var tokenData = JwtHelper.ParseToken(accessToken);
        if (tokenData?.IsValid == true)
        {
            var (instituteId, fullName) = await _authService.GetUserDetailsAsync(tokenData.Username!);
            await SetUserClaims(tokenData, instituteId, fullName);
        }
        
        return Ok();
    }

    private async Task SetUserClaims(JwtTokenData tokenData, int? instituteId, string? fullName)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, tokenData.UserId ?? ""),
            new Claim(ClaimTypes.Name, tokenData.Username ?? ""),
            new Claim(ClaimTypes.Email, tokenData.Email ?? ""),
            new Claim(ClaimTypes.Role, tokenData.Role ?? ""),
            new Claim("InstituteId", instituteId?.ToString() ?? "0")
        };
        
        if (!string.IsNullOrEmpty(fullName))
            claims.Add(new Claim("FullName", fullName));
        
        await HttpContext.SignInAsync("SessionScheme", new ClaimsPrincipal(new ClaimsIdentity(claims, "SessionScheme")));
    }
}
