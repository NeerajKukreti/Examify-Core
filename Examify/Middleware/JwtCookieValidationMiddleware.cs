using Microsoft.AspNetCore.Authentication;

namespace Examify.Middleware
{
    public class JwtCookieValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtCookieValidationMiddleware> _logger;

        public JwtCookieValidationMiddleware(RequestDelegate next, ILogger<JwtCookieValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip for login/logout pages and static files
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("/auth/") || path.Contains("/login") || 
                path.Contains(".css") || path.Contains(".js") || path.Contains(".png"))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated via SessionScheme
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var jwtToken = context.Request.Cookies["JWToken"];
                
                // JWT cookie missing but user claims exist
                if (string.IsNullOrEmpty(jwtToken))
                {
                    // Try to restore from SessionScheme claims
                    var tokenFromClaim = context.User.FindFirst("JWToken")?.Value;
                    var refreshTokenFromClaim = context.User.FindFirst("RefreshToken")?.Value;
                    
                    if (!string.IsNullOrEmpty(tokenFromClaim) && !string.IsNullOrEmpty(refreshTokenFromClaim))
                    {
                        _logger.LogInformation("Restoring JWT cookies from SessionScheme claims. UserId: {UserId}", 
                            context.User.Identity.Name);
                        
                        // Restore cookies
                        var isSecure = !context.Request.IsHttps ? false : true;
                        
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = isSecure,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddHours(3)
                        };

                        var refreshCookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = isSecure,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddDays(7)
                        };

                        context.Response.Cookies.Append("JWToken", tokenFromClaim, cookieOptions);
                        context.Response.Cookies.Append("RefreshToken", refreshTokenFromClaim, refreshCookieOptions);
                    }
                    else
                    {
                        _logger.LogWarning("User authenticated but JWT cookie and claims missing. UserId: {UserId}", 
                            context.User.Identity.Name);
                        
                        // No backup available - sign out and redirect
                        await context.SignOutAsync("SessionScheme");
                        context.Response.Redirect("/Auth/Index?reason=token_expired");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class JwtCookieValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookieValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieValidationMiddleware>();
        }
    }
}
