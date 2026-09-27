using DAL.Repository;
using DataModel;
using ExamifyAPI.Services;
using Microsoft.IdentityModel.Tokens;
using Model.DTO;
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
        Task<AuthResponse?> RegisterPersonalAsync(PersonalRegisterDTO dto);
        int GetCurrentUserID();
        int GetCurrentInstituteId();
        int GetCurrentTenantType();
        Task<User?> GetUserByUsernameAsync(string username);
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTime AccessTokenExpires { get; set; }
        public DateTime RefreshTokenExpires { get; set; }
        public int InstituteId { get; set; }
        public string FullName { get; set; } = "";
        public int TenantType { get; set; } = 2; // 1 = Personal, 2 = Institute
        public string PlanType { get; set; } = "Free";
    }
    
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IInstituteRepository _instituteRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            IUserService userService, 
            IInstituteRepository instituteRepository,
            IStudentRepository studentRepository,
            IConfiguration config, 
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _instituteRepository = instituteRepository;
            _studentRepository = studentRepository;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponse?> Authenticate(string username, string password)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null || !_userService.VerifyPassword(password, user.PasswordHash))
                return null;

            var tokens = GenerateTokens(user.UserId.ToString(), user.Username, user.Role, user.InstituteId, user.TenantType);
            tokens.InstituteId = user.InstituteId;
            tokens.FullName = user.FullName;
            tokens.TenantType = user.TenantType;
            tokens.PlanType = user.PlanType;
            return tokens; 
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
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                var principal = handler.ValidateToken(refreshToken, validationParameters, out _);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                var instituteId = principal.FindFirst("InstituteId")?.Value;
                var tenantTypeClaim = principal.FindFirst("TenantType")?.Value;
                int.TryParse(tenantTypeClaim ?? "2", out var tenantType);

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                    return null;

                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                    return null;

                var tokens = GenerateTokens(userId, username, role ?? "Student", int.Parse(instituteId ?? "0"), tenantType);
                tokens.InstituteId = user.InstituteId;
                tokens.FullName = user.FullName;
                tokens.TenantType = user.TenantType;
                tokens.PlanType = user.PlanType;
                return tokens;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AuthResponse?> RegisterPersonalAsync(PersonalRegisterDTO dto)
        {
            var username = string.IsNullOrWhiteSpace(dto.Username) ? dto.Email : dto.Username;

            // Check if username/email already exists
            var existingUser = await _userService.GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                return null;
            }

            // 1. Create base User
            var newUserId = await _userService.CreateUserAsync(username, dto.Password, "Student");
            if (newUserId <= 0)
            {
                return null;
            }

            // 2. Create Personal Workspace (TenantType = 1, OwnerUserId = newUserId)
            var personalInstituteId = await _instituteRepository.CreatePersonalWorkspaceAsync(newUserId, dto.FullName, dto.Email);

            // 3. Create Student profile linked to Personal Workspace
            var studentId = await _studentRepository.CreatePersonalStudentProfileAsync(newUserId, personalInstituteId, dto.FullName, dto.Email);

            var targetUserId = studentId > 0 ? studentId : newUserId;

            // 4. Generate Tokens
            var tokens = GenerateTokens(targetUserId.ToString(), username, "Student", personalInstituteId, 1);
            tokens.InstituteId = personalInstituteId;
            tokens.FullName = dto.FullName;
            tokens.TenantType = 1;
            tokens.PlanType = "Free";

            return tokens;
        }

        private AuthResponse GenerateTokens(string userId, string username, string role, int instituteId, int tenantType = 2)
        {
            var accessTokenExpiry = DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:ExpireHours"] ?? "3"));
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            var accessToken = GenerateJwtToken(userId, username, role, instituteId, tenantType, accessTokenExpiry);
            var refreshToken = GenerateJwtToken(userId, username, role, instituteId, tenantType, refreshTokenExpiry);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpires = accessTokenExpiry,
                RefreshTokenExpires = refreshTokenExpiry,
                TenantType = tenantType,
                PlanType = "Free"
            };
        }

        private string GenerateJwtToken(string userId, string username, string role, int instituteId, int tenantType, DateTime expires)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("InstituteId", instituteId.ToString()),
                new Claim("TenantType", tenantType.ToString())
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
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            return userId;
        }

        public int GetCurrentInstituteId()
        {
            var instituteIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("InstituteId");
            return instituteIdClaim != null ? int.Parse(instituteIdClaim.Value) : 0;
        }

        public int GetCurrentTenantType()
        {
            var userId = GetCurrentUserID();
            if (userId > 0)
            {
                var userTask = _userService.GetUserByUsernameAsync(_httpContextAccessor.HttpContext?.User.Identity?.Name ?? "");
                var user = userTask.GetAwaiter().GetResult();
                if (user != null)
                {
                    return user.TenantType;
                }
            }

            var tenantClaim = _httpContextAccessor.HttpContext?.User.FindFirst("TenantType");
            return tenantClaim != null && int.TryParse(tenantClaim.Value, out var tenantType) ? tenantType : 2;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userService.GetUserByUsernameAsync(username);
        }
    }
}