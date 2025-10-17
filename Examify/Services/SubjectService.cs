using DataModel;
using Examify.Common;
using Microsoft.Extensions.Options;
using Model.DTO;
using Newtonsoft.Json;
using System.Text;

namespace Examify.Services
{
    public interface ISubjectService
    {
        Task<IEnumerable<SubjectModel>> GetAllAsync(int instituteId, int? subjectId = null);
        Task<SubjectModel> GetByIdAsync(int subjectId, int instituteId);
        Task<IEnumerable<SubjectTopicModel>> GetTopicsBySubjectIdAsync(int subjectId);
        Task<bool> CreateAsync(SubjectDTO model);
        Task<bool> UpdateAsync(SubjectDTO model);
        Task<bool> ChangeStatusAsync(int subjectId);
    }

    public class SubjectService : ISubjectService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public SubjectService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient("ExamifyAPI");
            _apiSettings = apiSettings.Value;
        }

        public async Task<IEnumerable<SubjectModel>> GetAllAsync(int instituteId, int? subjectId = null)
        {
            try
            {
                var endpoint = subjectId.HasValue
                    ? $"Subject/{instituteId}/{subjectId}"
                    : $"Subject/{instituteId}/0";

                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);

                    if (apiResponse?.Success == true && apiResponse?.Data != null)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<SubjectModel>>(apiResponse.Data.ToString());
                    }
                }

                return new List<SubjectModel>();
            }
            catch (Exception ex)
            {
                return new List<SubjectModel>();
            }
        }

        public async Task<SubjectModel> GetByIdAsync(int subjectId, int instituteId)
        {
            try
            {
                var subjects = await GetAllAsync(instituteId, subjectId);
                return subjects?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<SubjectTopicModel>> GetTopicsBySubjectIdAsync(int subjectId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Subject/{subjectId}/topics");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);

                    if (apiResponse?.Success == true && apiResponse?.Data != null)
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<SubjectTopicModel>>(apiResponse.Data.ToString());
                    }
                }

                return new List<SubjectTopicModel>();
            }
            catch (Exception ex)
            {
                return new List<SubjectTopicModel>();
            }
        }

        public async Task<bool> CreateAsync(SubjectDTO model)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("Subject", content);

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
                return false;
            }
        }

        public async Task<bool> UpdateAsync(SubjectDTO model)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"Subject/{model.SubjectId}", content);

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
                return false;
            }
        }

        public async Task<bool> ChangeStatusAsync(int subjectId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"Subject/ChangeStatus?id={subjectId}", null);

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
                return false;
            }
        }
    }
}
