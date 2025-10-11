using DataModel;
using Examify.Common;
using Microsoft.Extensions.Options;
using Model.DTO;
using Newtonsoft.Json;
using System.Text;

namespace Examify.Services
{
    public interface IClassService
    {
        Task<IEnumerable<ClassDTO>> GetAllAsync(int instituteId);
        Task<ClassDTO?> GetByIdAsync(int instituteId, int classId);
        Task<bool> CreateAsync(ClassDTO classDto);
        Task<bool> UpdateAsync(ClassDTO classDto);
        Task<bool> DeleteAsync(int classId);
        Task<bool> ChangeStatusAsync(int classId);
    }

    public class ClassService : IClassService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public ClassService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient("ExamifyAPI");
            _apiSettings = apiSettings.Value;
        }

        public async Task<IEnumerable<ClassDTO>> GetAllAsync(int instituteId)
        {
            try
            {
                var endpoint = $"Class/GetAll/{instituteId}";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<ClassDTO>>>(content);
                    
                    if (apiResponse?.Success == true && apiResponse?.Data != null)
                    {
                        return apiResponse.Data;
                    }
                }
                
                return new List<ClassDTO>();
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"GetAllAsync failed: {ex.Message}");
                return new List<ClassDTO>();
            }
        }

        public async Task<ClassDTO?> GetByIdAsync(int instituteId, int classId)
        {
            try
            {
                var endpoint = $"Class/GetAll/{instituteId}/{classId}";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<ClassDTO>>>(content);
                    
                    if (apiResponse?.Success == true && apiResponse?.Data != null)
                    {
                        // Since the API returns an array, get the first item (should be the requested class)
                        return apiResponse.Data.FirstOrDefault();
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"GetByIdAsync failed: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateAsync(ClassDTO classDto)
        {
            try
            {
                var json = JsonConvert.SerializeObject(classDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("Class", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
                    return apiResponse?.Success == true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"CreateAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ClassDTO classDto)
        {
            try
            {
                var json = JsonConvert.SerializeObject(classDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"Class/{classDto.ClassId}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
                    return apiResponse?.Success == true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"UpdateAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int classId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Class/{classId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);
                    return apiResponse?.Success == true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"DeleteAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ChangeStatusAsync(int classId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"Class/ChangeStatus?id={classId}", null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<object>>(content);
                    return apiResponse?.Success == true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"ChangeStatusAsync failed: {ex.Message}");
                return false;
            }
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}