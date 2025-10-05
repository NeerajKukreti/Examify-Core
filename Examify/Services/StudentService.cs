using DataModel;
using Examify.Common;
using Microsoft.Extensions.Options;
using Model.DTO;
using Newtonsoft.Json;
using System.Text;

namespace Examify.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentModel>> GetAllAsync(int instituteId, int? studentId = null);
        Task<StudentModel> GetByIdAsync(int studentId, int instituteId);
        Task<bool> CreateAsync(StudentDTO model);
        Task<bool> UpdateAsync(StudentDTO model);
        Task<bool> DeleteAsync(int studentId);
        Task<bool> ChangeStatusAsync(int studentId);
    }

    public class StudentService : IStudentService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public StudentService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient("ExamifyAPI");
            _apiSettings = apiSettings.Value;
        }

        public async Task<IEnumerable<StudentModel>> GetAllAsync(int instituteId, int? studentId = null)
        {
            try
            {
                var endpoint = studentId.HasValue 
                    ? $"Student/{instituteId}/{studentId}" 
                    : $"Student/{instituteId}/0";
                    
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);
                    
                    if (apiResponse?.Success == true && apiResponse?.Data != null)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<StudentModel>>(apiResponse.Data.ToString());
                    }
                }
                
                return new List<StudentModel>();
            }
            catch (Exception ex)
            {
                // Log exception
                return new List<StudentModel>();
            }
        }

        public async Task<StudentModel> GetByIdAsync(int studentId, int instituteId)
        {
            try
            {
                var students = await GetAllAsync(instituteId, studentId);
                return students?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Log exception
                return null;
            }
        }

        public async Task<bool> CreateAsync(StudentDTO model)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("Student", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return apiResponse?.Success == true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // Log exception
                return false;
            }
        }

        public async Task<bool> UpdateAsync(StudentDTO model)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"Student/{model.StudentId}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return apiResponse?.Success == true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                // Log exception
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int studentId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Student/{studentId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                return false;
            }
        }
        public async Task<bool> ChangeStatusAsync(int studentId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"Student/ChangeStatus?id={studentId}", null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);
                    return apiResponse?.Success == true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log exception
                return false;
            }
        }
    }
}