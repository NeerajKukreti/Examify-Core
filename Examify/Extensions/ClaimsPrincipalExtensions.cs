using System.Security.Claims;

namespace Examify.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetInstituteId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst("InstituteId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string? GetUserName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string? GetEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string? GetRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Role)?.Value;
        }

        public static string? GetFullName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("FullName")?.Value ?? principal.GetUserName();
        }
    }
}
