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
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IHttpClientFactory httpClientFactory, 
            IHttpContextAccessor httpContextAccessor, 
            IOptions<ApiSettings> apiSettings,
            IWebHostEnvironment environment,
            ILogger<AuthService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
            _environment = environment;
            _logger = logger;
        }

        public async Task<(bool Success, string? Token, string? RefreshToken, int? InstituteId, string? FullName, string? ErrorMessage)> 
            LoginAsync(UserDTO dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ExamifyAPI");

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("auth/login", content);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, null, null, null, null, "Invalid credentials");
                }

                var result = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<JsonElement>(result);
                var token = authResult.GetProperty("AccessToken").GetString();
                var refreshToken = authResult.GetProperty("RefreshToken").GetString();
                var instituteId = authResult.TryGetProperty("InstituteId", out var instId) ? instId.GetInt32() : (int?)null;
                var fullName = authResult.TryGetProperty("FullName", out var name) ? name.GetString() : null;

                return (true, token, refreshToken, instituteId, fullName, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, null, null, ex.Message);
            }
        }

        public void SaveTokensInSession(string token, string refreshToken)
        {
            try
            {
                var isSecure = !_environment.IsDevelopment();
                
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

                _httpContextAccessor.HttpContext!.Response.Cookies.Append("JWToken", token, cookieOptions);
                _httpContextAccessor.HttpContext!.Response.Cookies.Append("RefreshToken", refreshToken, refreshCookieOptions);
                
                _logger.LogInformation("Tokens saved to cookies successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save tokens to cookies");
                throw;
            }
        }

        public void ClearSession()
        {
            try
            {
                var response = _httpContextAccessor.HttpContext?.Response;
                if (response == null) return;

                response.Cookies.Delete("JWToken");
                response.Cookies.Delete("RefreshToken");
                
                _logger.LogInformation("Tokens cleared from cookies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear tokens from cookies");
            }
        }

        public JwtTokenData? ParseToken(string? token) => JwtHelper.ParseToken(token);

        public async Task<(int? InstituteId, string? FullName)> GetUserDetailsAsync(string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ExamifyAPI");
                var response = await client.GetAsync($"auth/user/{username}");
                
                if (!response.IsSuccessStatusCode)
                    return (null, null);

                var result = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<JsonElement>(result);
                var instituteId = userData.TryGetProperty("InstituteId", out var instId) ? instId.GetInt32() : (int?)null;
                var fullName = userData.TryGetProperty("FullName", out var name) ? name.GetString() : null;

                return (instituteId, fullName);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}