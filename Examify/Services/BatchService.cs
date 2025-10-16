using DataModel;
using Examify.Common;
using Microsoft.Extensions.Options;
using Model.DTO;
using Newtonsoft.Json;
using System.Text;

namespace Examify.Services
{
    public interface IBatchService
    {
        Task<IEnumerable<BatchDTO>> GetAllAsync(int classId);
        Task<BatchDTO?> GetByIdAsync(int batchId);
        Task<IEnumerable<BatchDTO>> GetBatchesByClassIdAsync(int classId);
        Task<bool> CreateAsync(BatchDTO batchDto);
        Task<bool> UpdateAsync(BatchDTO batchDto);
        Task<bool> DeleteAsync(int batchId);
        Task<bool> ChangeStatusAsync(int batchId);
    }

    public class BatchService : IBatchService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public BatchService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient("ExamifyAPI");
            _apiSettings = apiSettings.Value;
        }

        public async Task<IEnumerable<BatchDTO>> GetAllAsync(int classId)
        {
            try
            {
                var endpoint = $"Batch/GetAll/{classId}";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<BatchDTO>>>(content);
                    
                    if (apiResponse?.Success == true && apiResponse?.Data != null)
                    {
                        return apiResponse.Data;
                    }
                }
                
                return new List<BatchDTO>();
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"GetAllAsync failed: {ex.Message}");
                return new List<BatchDTO>();
            }
        }

        public async Task<BatchDTO?> GetByIdAsync(int batchId)
        {
            try
            {
                var endpoint = $"Batch/{batchId}";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<BatchDTO>(content);
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

        public async Task<IEnumerable<BatchDTO>> GetBatchesByClassIdAsync(int classId)
        {
            try
            {
                var endpoint = $"Batch/ByClass/{classId}";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<IEnumerable<BatchDTO>>>(content);
                    
                    if (apiResponse?.Success == true && apiResponse?.Data != null)
                    {
                        return apiResponse.Data;
                    }
                }
                
                return new List<BatchDTO>();
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"GetBatchesByClassIdAsync failed: {ex.Message}");
                return new List<BatchDTO>();
            }
        }

        public async Task<bool> CreateAsync(BatchDTO batchDto)
        {
            try
            {
                var json = JsonConvert.SerializeObject(batchDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("Batch", content);
                
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

        public async Task<bool> UpdateAsync(BatchDTO batchDto)
        {
            try
            {
                var json = JsonConvert.SerializeObject(batchDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"Batch/{batchDto.BatchId}", content);
                
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

        public async Task<bool> DeleteAsync(int batchId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Batch/{batchId}");
                
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

        public async Task<bool> ChangeStatusAsync(int batchId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"Batch/ChangeStatus?id={batchId}", null);

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
}