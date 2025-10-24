using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using DataModel;

namespace Examify.Attributes
{
    public class AutoLoginAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;
        private readonly bool _requireSession;
        private readonly string? _defaultUsername;
        private readonly string? _defaultPassword;

        public AutoLoginAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? new string[0];
            _requireSession = true;
            _defaultUsername = "admin";
            _defaultPassword = "admin123";
        }

        public AutoLoginAuthorizeAttribute(string username, string password, params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? new string[0];
            _requireSession = true;
            _defaultUsername = username;
            _defaultPassword = password;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            if (IsSessionValid(httpContext))
                return;

            var autoLoginResult = PerformCredentialAutoLogin(httpContext);
            if (autoLoginResult)
                return;

            RedirectToLogin(context);
        }

        private bool IsSessionValid(HttpContext httpContext)
        {
            try
            {
                var jwtToken = httpContext.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    try
                    {
                        var tokenData = Examify.Common.JwtHelper.DecodeToken(jwtToken);
                        if (tokenData != null && !string.IsNullOrEmpty(tokenData.UserId))
                        {
                            if (_allowedRoles.Length > 0)
                            {
                                var sessionRole = httpContext.Session.GetString("UserRole");
                                return !string.IsNullOrEmpty(sessionRole) && 
                                       _allowedRoles.Contains(sessionRole, StringComparer.OrdinalIgnoreCase);
                            }
                            return true;
                        }
                    }
                    catch { }
                }

                var userId = httpContext.Session.GetInt32("UserId");
                var userRole = httpContext.Session.GetString("UserRole");

                if (!_requireSession) return true;
                if (userId == null || userId <= 0) return false;

                if (_allowedRoles.Length > 0 && !string.IsNullOrEmpty(userRole))
                    return _allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool PerformCredentialAutoLogin(HttpContext httpContext)
        {
            try
            {
                var username = httpContext.Request.Query["username"].FirstOrDefault() ?? _defaultUsername;
                var password = httpContext.Request.Query["password"].FirstOrDefault() ?? _defaultPassword;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    return false;

                var authResult = AuthenticateWithRealAPI(httpContext, username, password).Result;
                
                if (authResult.Success && authResult.User != null)
                {
                    SetSessionDataWithJWT(httpContext, authResult);
                    SetAuthenticationCookies(httpContext, authResult);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<AuthenticationResult> AuthenticateWithRealAPI(HttpContext httpContext, string username, string password)
        {
            try
            {
                var userDto = new UserDTO { Username = username, Password = password, Role = "Admin" };
                var authService = httpContext.RequestServices.GetService<Examify.Services.AuthService>();
                
                if (authService != null)
                {
                    var loginResult = await authService.LoginAsync(userDto);
                    
                    if (loginResult.Success && !string.IsNullOrEmpty(loginResult.Token))
                    {
                        var tokenData = Examify.Common.JwtHelper.DecodeToken(loginResult.Token);
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(loginResult.Token);
                        var role = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value ?? "Admin";
                        
                        return new AuthenticationResult
                        {
                            Success = true,
                            Message = "Authentication successful",
                            User = new UserSessionData
                            {
                                UserId = int.Parse(tokenData.UserId ?? "0"),
                                UserName = tokenData.Username ?? username,
                                Email = tokenData.Email ?? "",
                                UserRole = role,
                                InstituteId = 3,
                                StudentId = null,
                                IsActive = true
                            },
                            AuthToken = loginResult.Token,
                            RefreshToken = loginResult.RefreshToken
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Authentication failed: {ex.Message}");
            }
            
            return new AuthenticationResult { Success = false, Message = "Authentication failed" };
        }

        private void SetAuthenticationCookies(HttpContext httpContext, AuthenticationResult authResult)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(30)
            };

            if (!string.IsNullOrEmpty(authResult.AuthToken))
                httpContext.Response.Cookies.Append("AuthToken", authResult.AuthToken, cookieOptions);

            if (!string.IsNullOrEmpty(authResult.RefreshToken))
                httpContext.Response.Cookies.Append("RefreshToken", authResult.RefreshToken, cookieOptions);

            httpContext.Response.Cookies.Append("RememberMe", "true", cookieOptions);
        }

        private void SetSessionDataWithJWT(HttpContext httpContext, AuthenticationResult authResult)
        {
            if (!string.IsNullOrEmpty(authResult.AuthToken))
                httpContext.Session.SetString("JWToken", authResult.AuthToken);
            
            if (!string.IsNullOrEmpty(authResult.RefreshToken))
                httpContext.Session.SetString("RefreshToken", authResult.RefreshToken);
            
            if (authResult.User != null)
            {
                httpContext.Session.SetInt32("UserId", authResult.User.UserId);
                httpContext.Session.SetString("UserName", authResult.User.UserName ?? "");
                httpContext.Session.SetString("UserRole", authResult.User.UserRole ?? "");
                httpContext.Session.SetString("Email", authResult.User.Email ?? "");
                
                if (authResult.User.InstituteId.HasValue)
                    httpContext.Session.SetInt32("InstituteId", authResult.User.InstituteId.Value);
                
                if (authResult.User.StudentId.HasValue)
                    httpContext.Session.SetInt32("StudentId", authResult.User.StudentId.Value);
            }
        }

        private void RedirectToLogin(AuthorizationFilterContext context)
        {
            var loginUrl = "/Login";
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl != "/")
                loginUrl += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";

            context.Result = new RedirectResult(loginUrl);
        }
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserSessionData? User { get; set; }
        public string? AuthToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class UserSessionData
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? UserRole { get; set; }
        public int? InstituteId { get; set; }
        public int? StudentId { get; set; }
        public bool IsActive { get; set; }
    }
}
