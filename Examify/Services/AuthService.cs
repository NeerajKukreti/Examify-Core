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

        public async Task<(bool Success, string? Token, string? UserId, string? ErrorMessage)> LoginAsync(UserDTO dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(_apiSettings.ClientName);

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_apiSettings.Endpoints.Login, content);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, null, null, "Invalid credentials");
                }

                var result = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<JsonElement>(result);
                var token = authResult.GetProperty("Token").GetString();

                // Decode JWT to get user data
                var tokenData = JwtHelper.DecodeToken(token!);

                return (true, token, tokenData.UserId, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, ex.Message);
            }
        }

        public void SaveSession(string token, string? userId)
        {
            _httpContextAccessor.HttpContext!.Session.SetString("JWToken", token);
            if (userId != null)
            {
                _httpContextAccessor.HttpContext!.Session.SetString("UserId", userId);
            }
        }

        public void ClearSession()
        {
            _httpContextAccessor.HttpContext!.Session.Remove("JWToken");
            _httpContextAccessor.HttpContext!.Session.Remove("UserId");
        }
    }
}