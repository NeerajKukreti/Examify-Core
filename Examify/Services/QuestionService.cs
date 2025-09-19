using DataModel;
using Microsoft.Extensions.Options;
using OnlineExam.Common;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        private readonly ApiSettings _apiSettings;

        public QuestionService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
        }

        public async Task<List<QuestionModel>?> GetAllAsync()
        {
            var client = _httpClientFactory.CreateClient(Constants.ClientName);
            return await client.GetFromJsonAsync<List<QuestionModel>>(Constants.QuestionList);
        }

        public async Task<QuestionModel?> GetByIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient(_apiSettings.ClientName);
            return await client.GetFromJsonAsync<QuestionModel>(string.Format(_apiSettings.Endpoints.QuestionBankById, id));
        }

        public async Task<bool> CreateAsync(QuestionModel model)
        {
            var client = _httpClientFactory.CreateClient(_apiSettings.ClientName);
            var response = await client.PostAsJsonAsync(_apiSettings.Endpoints.QuestionBankCreate, model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(QuestionModel model)
        {
            var client = _httpClientFactory.CreateClient(_apiSettings.ClientName);
            var response = await client.PutAsJsonAsync(_apiSettings.Endpoints.QuestionBankUpdate, model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var client = _httpClientFactory.CreateClient(_apiSettings.ClientName);
            var response = await client.DeleteAsync(string.Format(_apiSettings.Endpoints.QuestionBankDelete, id));
            return response.IsSuccessStatusCode;
        }
    }
}
