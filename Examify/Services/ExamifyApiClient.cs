using DataModel;
using DataModel.Common;
using Model.DTO;
using System.Text.Json;
using System.Text;

namespace Examify.Services
{
    public interface IExamifyApiClient
    {
        Task<ApiResponse<List<ExamModel>>> GetPublicCatalogAsync();
        Task<ApiResponse<ExamModel>> GetExamByIdAsync(int examId);
        Task<ApiResponse<ExamModel>> GetSessionExamByIdAsync(int examId);
    }

    public class ExamifyApiClient : IExamifyApiClient
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ExamifyApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<ApiResponse<List<ExamModel>>> GetPublicCatalogAsync()
        {
            try
            {
                var response = await _client.GetAsync("Exam/catalog");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiResponse<List<ExamModel>>>(json, _jsonOptions) 
                       ?? new ApiResponse<List<ExamModel>> { Success = false };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<ExamModel>> { Success = false, Message = ex.Message, Data = new List<ExamModel>() };
            }
        }

        public async Task<ApiResponse<ExamModel>> GetExamByIdAsync(int examId)
        {
            try
            {
                var response = await _client.GetAsync($"Exam/{examId}");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiResponse<ExamModel>>(json, _jsonOptions) 
                       ?? new ApiResponse<ExamModel> { Success = false };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ExamModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<ExamModel>> GetSessionExamByIdAsync(int examId)
        {
            try
            {
                var response = await _client.GetAsync($"Exam/Session/{examId}");
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiResponse<ExamModel>>(json, _jsonOptions) 
                       ?? new ApiResponse<ExamModel> { Success = false };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ExamModel> { Success = false, Message = ex.Message };
            }
        }
    }
}
