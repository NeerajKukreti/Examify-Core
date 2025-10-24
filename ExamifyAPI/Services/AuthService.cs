// Services/AuthService.cs
using DataModel;
using ExamifyAPI.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExamAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> Authenticate(string username, string password);
        Task<AuthResponse?> RefreshToken(string refreshToken);
        Task<int> Register(string username, string password, string role);
        int GetCurrentUserID();
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime AccessTokenExpires { get; set; }
        public DateTime RefreshTokenExpires { get; set; }
    }
    
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUserService userService, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponse?> Authenticate(string username, string password)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null || !_userService.VerifyPassword(password, user.PasswordHash))
                return null;

            return GenerateTokens(user.UserId.ToString(), user.Username, user.Role);
        }

        public async Task<AuthResponse?> RefreshToken(string refreshToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                var principal = handler.ValidateToken(refreshToken, validationParameters, out _);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                    return null;

                return GenerateTokens(userId, username, role);
            }
            catch
            {
                return null;
            }
        }

        private AuthResponse GenerateTokens(string userId, string username, string role)
        {
            var accessTokenExpiry = DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:ExpireHours"] ?? "3"));
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            var accessToken = GenerateJwtToken(userId, username, role, accessTokenExpiry);
            var refreshToken = GenerateJwtToken(userId, username, role, refreshTokenExpiry);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpires = accessTokenExpiry,
                RefreshTokenExpires = refreshTokenExpiry
            };
        }

        private string GenerateJwtToken(string userId, string username, string role, DateTime expires)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<int> Register(string username, string password, string role)
        {
            return await _userService.CreateUserAsync(username, password, role);
        }
        
        public int GetCurrentUserID()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // fallback for Swagger/Postman testing without login
                return 1;
            }
            return int.Parse(userIdClaim.Value);
        }
    }
}