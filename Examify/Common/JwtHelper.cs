using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Examify.Common
{
    public static class JwtHelper
    {
        public static JwtTokenData? ParseToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);

                return new JwtTokenData
                {
                    UserId = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                    Username = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value,
                    Email = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value,
                    Role = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
                    ExpiryDate = jsonToken.ValidTo,
                    IsValid = jsonToken.ValidTo > DateTime.UtcNow,
                    IsExpired = jsonToken.ValidTo <= DateTime.UtcNow,
                    Token = token
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public class JwtTokenData
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public string? Token { get; set; }

        public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
        public bool IsStudent => string.Equals(Role, "Student", StringComparison.OrdinalIgnoreCase);
        public bool IsInstitute => string.Equals(Role, "Institute", StringComparison.OrdinalIgnoreCase);
        public TimeSpan? TimeUntilExpiry => ExpiryDate.HasValue ? ExpiryDate.Value - DateTime.UtcNow : null;
        public bool IsNearExpiry => TimeUntilExpiry.HasValue && TimeUntilExpiry.Value.TotalMinutes < 30;
    }
}