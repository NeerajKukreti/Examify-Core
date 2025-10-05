using DataModel;
using Examify.Common;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Examify.Services
{
    public interface IStateService
    {
        Task<List<StateModel>> GetAllAsync();
    }

    public class StateService : IStateService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;

        public StateService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClientFactory.CreateClient("ExamifyAPI");
            _apiSettings = apiSettings.Value;
        }

        public async Task<List<StateModel>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("State");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(content);
                    
                    if (apiResponse?.Success == true && apiResponse?.Data != null)
                    {
                        var states = JsonConvert.DeserializeObject<List<StateModel>>(apiResponse.Data.ToString());
                        return states ?? new List<StateModel>();
                    }
                }
                
                return new List<StateModel>();
            }
            catch (Exception ex)
            {
                // Log exception
                return new List<StateModel>();
            }
        }

        // Keep the old method for backward compatibility
        [Obsolete("Use GetAllAsync() instead")]
        public async Task<List<StateModel>> GetState()
        {
            return await GetAllAsync();
        }
    }
}
