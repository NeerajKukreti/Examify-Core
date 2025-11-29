using Model.DTO;
using DataModel.Common;
using System.Text;

namespace Examify.Services
{
    public interface IRandomizationPresetService
    {
        Task<IEnumerable<RandomizationPresetDTO>> GetAllPresetsAsync(int instituteId);
        Task<RandomizationPresetDTO?> GetPresetByIdAsync(int presetId);
        Task<int> CreatePresetAsync(RandomizationPresetDTO preset);
        Task<bool> UpdatePresetAsync(RandomizationPresetDTO preset);
        Task<bool> DeletePresetAsync(int presetId);
        Task<PresetPreviewDTO> PreviewPresetAsync(int presetId, int instituteId);
        Task<List<int>> ExecutePresetAsync(PresetExecutionDTO execution);
    }

    public class RandomizationPresetService : IRandomizationPresetService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public RandomizationPresetService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private string ApiUrl => _config["ExamifyAPI:BaseUrl"] ?? "https://localhost:7271/api/";

        public async Task<IEnumerable<RandomizationPresetDTO>> GetAllPresetsAsync(int instituteId)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var response = await client.GetFromJsonAsync<ApiResponse<IEnumerable<RandomizationPresetDTO>>>($"RandomizationPreset/{instituteId}");
            return response?.Data ?? Enumerable.Empty<RandomizationPresetDTO>();
        }

        public async Task<RandomizationPresetDTO?> GetPresetByIdAsync(int presetId)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var response = await client.GetFromJsonAsync<ApiResponse<RandomizationPresetDTO>>($"RandomizationPreset/detail/{presetId}");
            return response?.Data;
        }

        public async Task<int> CreatePresetAsync(RandomizationPresetDTO preset)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var response = await client.PostAsJsonAsync("RandomizationPreset", preset);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            return result?.Data ?? 0;
        }

        public async Task<bool> UpdatePresetAsync(RandomizationPresetDTO preset)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var response = await client.PutAsJsonAsync("RandomizationPreset", preset);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result?.Success ?? false;
        }

        public async Task<bool> DeletePresetAsync(int presetId)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var response = await client.DeleteAsync($"RandomizationPreset/{presetId}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result?.Success ?? false;
        }

        public async Task<PresetPreviewDTO> PreviewPresetAsync(int presetId, int instituteId)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var response = await client.GetFromJsonAsync<ApiResponse<PresetPreviewDTO>>($"RandomizationPreset/preview/{presetId}/{instituteId}");
            return response?.Data ?? new PresetPreviewDTO();
        }

        public async Task<List<int>> ExecutePresetAsync(PresetExecutionDTO execution)
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var response = await client.PostAsJsonAsync("RandomizationPreset/execute", execution);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<int>>>();
            return result?.Data ?? new List<int>();
        }
    }
}
