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
        Task<string?> Authenticate(string username, string password);
        Task<int> Register(string username, string password, string role);
        int GetCurrentUserID();
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

        public async Task<string?> Authenticate(string username, string password)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null || !_userService.VerifyPassword(password, user.PasswordHash))
                return null;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expireHours = int.Parse(_config["Jwt:ExpireHours"] ?? "3");
        
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expireHours),
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