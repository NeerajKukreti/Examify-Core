using DAL.Repository;
using Model.DTO;

namespace ExamifyAPI.Services
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
        private readonly IRandomizationPresetRepository _repository;

        public RandomizationPresetService(IRandomizationPresetRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<RandomizationPresetDTO>> GetAllPresetsAsync(int instituteId) 
            => _repository.GetAllPresetsAsync(instituteId);

        public Task<RandomizationPresetDTO?> GetPresetByIdAsync(int presetId) 
            => _repository.GetPresetByIdAsync(presetId);

        public Task<int> CreatePresetAsync(RandomizationPresetDTO preset) 
            => _repository.CreatePresetAsync(preset);

        public Task<bool> UpdatePresetAsync(RandomizationPresetDTO preset) 
            => _repository.UpdatePresetAsync(preset);

        public Task<bool> DeletePresetAsync(int presetId) 
            => _repository.DeletePresetAsync(presetId);

        public Task<PresetPreviewDTO> PreviewPresetAsync(int presetId, int instituteId) 
            => _repository.PreviewPresetAsync(presetId, instituteId);

        public Task<List<int>> ExecutePresetAsync(PresetExecutionDTO execution) 
            => _repository.ExecutePresetAsync(execution);
    }
}
