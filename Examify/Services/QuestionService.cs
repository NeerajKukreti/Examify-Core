using DataModel;
using Examify.Common;
using Examify.Common.constants;
using System.Text.Json;

namespace Examify.Services
{
    public interface IQuestionService
    {
        Task<List<QuestionModel>?> GetAllAsync();
        Task<QuestionModel?> GetByIdAsync(int id);
        Task<bool> CreateAsync(QuestionModel model);
        Task<bool> UpdateAsync(QuestionModel model);
        Task<bool> DeleteAsync(int id);
    }
    public class QuestionService : IQuestionService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public QuestionService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<QuestionModel>?> GetAllAsync()
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            return await client.GetFromJsonAsync<List<QuestionModel>>(ENDPOINTS.QuestionList);
        }

        public async Task<QuestionModel?> GetByIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            return await client.GetFromJsonAsync<QuestionModel>($"{ENDPOINTS.QuestionById}/{id}");
        }

        public async Task<bool> CreateAsync(QuestionModel model)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");

            // Debug: log the payload
            var json = JsonSerializer.Serialize(model);
            Console.WriteLine(json); // Or use a logger

            var response = await client.PostAsJsonAsync(ENDPOINTS.QuestionCreate, model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(QuestionModel model)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var response = await client.PutAsJsonAsync(ENDPOINTS.QuestionUpdate, model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var response = await client.DeleteAsync(string.Format(ENDPOINTS.QuestionDelete, id));
            return response.IsSuccessStatusCode;
        }
    }
}
