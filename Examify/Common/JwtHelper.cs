using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace OnlineExam.Common
{
    public static class JwtHelper
    {
        public static JwtTokenData DecodeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            
            return new JwtTokenData
            {
                UserId = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                Email = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value,
                Username = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value
            };
        }

        public static string GetUserIdFromSession(IHttpContextAccessor httpContextAccessor)
        {
            var token = httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return "1";

            try
            {
                var tokenData = DecodeToken(token);
                return tokenData.UserId ?? "1";
            }
            catch
            {
                return "1";
            }
        }
    }

    public class JwtTokenData
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
    }
}