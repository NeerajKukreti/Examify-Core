using DataModel;
using Examify.Common;
using Microsoft.Extensions.Options;
using Model.DTO;
using Newtonsoft.Json;
using System.Text;

namespace Examify.Services
{
    public interface IExamService
    {
        Task<IEnumerable<ExamModel>> GetAllAsync();
        Task<ExamModel> GetByIdAsync(int examId);
        Task<bool> CreateAsync(ExamDTO model);
        Task<bool> UpdateAsync(ExamDTO model);
        Task<bool> ChangeStatusAsync(int examId);
        Task<IEnumerable<AvailableQuestionDTO>> GetAvailableQuestionsAsync(int examId, int instituteId);
        Task<IEnumerable<ExamQuestionDTO>> GetExamQuestionsAsync(int examId);
        Task<bool> SaveExamQuestionsAsync(ExamQuestionConfigDTO config);
        Task<bool> RemoveExamQuestionAsync(int examId, int questionId);
    }

    public class ExamService : IExamService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public ExamService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient("ExamifyAPI");
            _apiSettings = apiSettings.Value;
        }

        public async Task<IEnumerable<ExamModel>> GetAllAsync()
        {

            var response = await _httpClient.GetAsync("Exam/list");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);

                if (apiResponse?.Success == true && apiResponse?.Data != null)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<ExamModel>>(apiResponse.Data.ToString());
                }
            }

            return new List<ExamModel>();

        }

        public async Task<ExamModel> GetByIdAsync(int examId)
        {
            var response = await _httpClient.GetAsync($"Exam/{examId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);

                if (apiResponse?.Success == true && apiResponse?.Data != null)
                {
                    return JsonConvert.DeserializeObject<ExamModel>(apiResponse.Data.ToString());
                }
                
            }
            return new ExamModel();
        }

        public async Task<bool> CreateAsync(ExamDTO model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Exam", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                return apiResponse?.Success == true;
            }

            return false; 
        }

        public async Task<bool> UpdateAsync(ExamDTO model)
        {
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"Exam/{model.ExamId}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                return apiResponse?.Success == true;
            }

            return false;

        }

        public async Task<bool> ChangeStatusAsync(int examId)
        {
            var response = await _httpClient.PutAsync($"Exam/ChangeStatus?id={examId}", null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);
                return apiResponse?.Success == true;
            }

            return false;
        }

        public async Task<IEnumerable<AvailableQuestionDTO>> GetAvailableQuestionsAsync(int examId, int instituteId)
        {
            var response = await _httpClient.GetAsync($"Exam/{examId}/available-questions?instituteId={instituteId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);

                if (apiResponse?.Success == true && apiResponse?.Data != null)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<AvailableQuestionDTO>>(apiResponse.Data.ToString());
                }
            }

            return new List<AvailableQuestionDTO>();
        }

        public async Task<IEnumerable<ExamQuestionDTO>> GetExamQuestionsAsync(int examId)
        {
            var response = await _httpClient.GetAsync($"Exam/{examId}/questions");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);

                if (apiResponse?.Success == true && apiResponse?.Data != null)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<ExamQuestionDTO>>(apiResponse.Data.ToString());
                }
            }

            return new List<ExamQuestionDTO>();
        }

        public async Task<bool> SaveExamQuestionsAsync(ExamQuestionConfigDTO config)
        {
            var json = JsonConvert.SerializeObject(config);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"Exam/{config.ExamId}/questions", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                return apiResponse?.Success == true;
            }

            return false;
        }

        public async Task<bool> RemoveExamQuestionAsync(int examId, int questionId)
        {
            var response = await _httpClient.DeleteAsync($"Exam/{examId}/questions/{questionId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);
                return apiResponse?.Success == true;
            }

            return false;
        }
    }
}
