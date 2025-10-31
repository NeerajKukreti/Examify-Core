using DataModel;
using Microsoft.Extensions.Options;
using Examify.Common;
using System.Text;
using System.Text.Json;

namespace Examify.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;

        public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
        }

        public async Task<(bool Success, string? Token, string? RefreshToken, string? UserId, string? ErrorMessage)> LoginAsync(UserDTO dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ExamifyAPI");

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("auth/login", content);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, null, null, null, "Invalid credentials");
                }

                var result = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<JsonElement>(result);
                var token = authResult.GetProperty("AccessToken").GetString();
                var refreshToken = authResult.GetProperty("RefreshToken").GetString();

                // Decode JWT to get user data
                var tokenData = JwtHelper.DecodeToken(token!);

                return (true, token, refreshToken, tokenData.UserId, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, null, ex.Message);
            }
        }

        public void SaveSession(string token, string refreshToken, string? userId)
        {
            try
            {
                // Parse token to get comprehensive user data and automatically set in session
                var userTokenData = ParseToken(token);
                
                if (userTokenData?.IsValid == true)
                {
                    // Store refresh token
                    _httpContextAccessor.HttpContext!.Session.SetString("RefreshToken", refreshToken);
                    System.Diagnostics.Debug.WriteLine($"Session saved for user: {userTokenData.Username}");
                }
                else
                {
                    // Fallback to basic session setting if token parsing fails
                    _httpContextAccessor.HttpContext!.Session.SetString("JWToken", token);
                    _httpContextAccessor.HttpContext!.Session.SetString("RefreshToken", refreshToken);
                    if (userId != null)
                    {
                        _httpContextAccessor.HttpContext!.Session.SetString("UserId", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save session: {ex.Message}");
                
                // Fallback to basic session setting
                _httpContextAccessor.HttpContext!.Session.SetString("JWToken", token);
                _httpContextAccessor.HttpContext!.Session.SetString("RefreshToken", refreshToken);
                if (userId != null)
                {
                    _httpContextAccessor.HttpContext!.Session.SetString("UserId", userId);
                }
            }
        }

        public void ClearSession()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return;

                // Clear all user-related session data
                session.Remove("JWToken");
                session.Remove("RefreshToken");
                session.Remove("UserId");
                session.Remove("UserIdInt");
                session.Remove("UserName");
                session.Remove("Email");
                session.Remove("UserRole");
                session.Remove("InstituteId");

                System.Diagnostics.Debug.WriteLine("All user session data cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear session: {ex.Message}");
            }
        }

        public UserTokenData? ParseToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return null;

                // Use existing JwtHelper to decode the token
                var tokenData = JwtHelper.DecodeToken(token);
                
                if (tokenData == null)
                    return null;

                // Parse the JWT token to get additional claims
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);

                // Extract additional claims
                var roleClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
                var instituteClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "InstituteId")?.Value;
                
                // Get expiry date
                var expiryDate = jsonToken.ValidTo;

                var userTokenData = new UserTokenData
                {
                    UserId = tokenData.UserId,
                    Username = tokenData.Username,
                    Email = tokenData.Email,
                    Role = roleClaim,
                    ExpiryDate = expiryDate,
                    IsValid = !string.IsNullOrEmpty(tokenData.UserId) && expiryDate > DateTime.UtcNow,
                    Token = token,
                    IsExpired = expiryDate <= DateTime.UtcNow
                };

                // Automatically set session data when parsing token
                if (userTokenData.IsValid && _httpContextAccessor.HttpContext?.Session != null)
                {
                    SetUserDataInSession(userTokenData);
                }

                return userTokenData;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                System.Diagnostics.Debug.WriteLine($"Token parsing failed: {ex.Message}");
                return null;
            }
        }

        private void SetUserDataInSession(UserTokenData userTokenData)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return;

                // Set JWT token
                if (!string.IsNullOrEmpty(userTokenData.Token))
                {
                    session.SetString("JWToken", userTokenData.Token);
                }

                // Set user ID
                if (!string.IsNullOrEmpty(userTokenData.UserId))
                {
                    session.SetString("UserId", userTokenData.UserId);
                    
                    // Also set as integer if it's a valid integer
                    if (int.TryParse(userTokenData.UserId, out var userIdInt))
                    {
                        session.SetInt32("UserIdInt", userIdInt);
                    }
                }

                // Set username
                if (!string.IsNullOrEmpty(userTokenData.Username))
                {
                    session.SetString("UserName", userTokenData.Username);
                }

                // Set email
                if (!string.IsNullOrEmpty(userTokenData.Email))
                {
                    session.SetString("Email", userTokenData.Email);
                }

                // Set role
                if (!string.IsNullOrEmpty(userTokenData.Role))
                {
                    session.SetString("UserRole", userTokenData.Role);
                }

                System.Diagnostics.Debug.WriteLine($"Session data set for user: {userTokenData.Username} (ID: {userTokenData.UserId}, Role: {userTokenData.Role})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set session data: {ex.Message}");
            }
        }

        public UserTokenData? ParseCurrentSessionToken()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
                return ParseToken(token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session token parsing failed: {ex.Message}");
                return null;
            }
        }

        public CurrentUserInfo GetCurrentUserInfo()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return new CurrentUserInfo();

                return new CurrentUserInfo
                {
                    UserId = session.GetString("UserId"),
                    UserIdInt = session.GetInt32("UserIdInt"),
                    Username = session.GetString("UserName"),
                    Email = session.GetString("Email"),
                    Role = session.GetString("UserRole"),
                    InstituteId = session.GetInt32("InstituteId"),
                    IsAuthenticated = !string.IsNullOrEmpty(session.GetString("JWToken")) && !string.IsNullOrEmpty(session.GetString("UserId"))
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get current user info: {ex.Message}");
                return new CurrentUserInfo();
            }
        }
    }

    // Current user information from session
    public class CurrentUserInfo
    {
        public string? UserId { get; set; }
        public int? UserIdInt { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? InstituteId { get; set; }
        public bool IsAuthenticated { get; set; }
        
        // Helper properties
        public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
        public bool IsTeacher => string.Equals(Role, "Teacher", StringComparison.OrdinalIgnoreCase);
        public bool IsStudent => string.Equals(Role, "Student", StringComparison.OrdinalIgnoreCase);
        public bool IsInstitute => string.Equals(Role, "Institute", StringComparison.OrdinalIgnoreCase);
    }

    // Token data model for parsed JWT information
    public class UserTokenData
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; } 

        public DateTime? ExpiryDate { get; set; }
        public bool IsValid { get; set; }
        public bool IsExpired { get; set; }
        public string? Token { get; set; }
        
        // Additional helper properties
        public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase); 
        public bool IsStudent => string.Equals(Role, "Student", StringComparison.OrdinalIgnoreCase);
        public bool IsInstitute => string.Equals(Role, "Institute", StringComparison.OrdinalIgnoreCase);
        
        public TimeSpan? TimeUntilExpiry => ExpiryDate.HasValue ? ExpiryDate.Value - DateTime.UtcNow : null;
        public bool IsNearExpiry => TimeUntilExpiry.HasValue && TimeUntilExpiry.Value.TotalMinutes < 30; // Expires in 30 minutes
    }
}