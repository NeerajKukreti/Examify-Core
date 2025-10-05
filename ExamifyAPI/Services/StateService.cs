using DAL.Repository;
using DataModel;

namespace ExamifyAPI.Services
{
    public interface IStateService
    {
        Task<IEnumerable<StateModel>> GetAllStatesAsync();
    }

    public class StateService : IStateService
    {
        private readonly IStateRepository _stateRepository;

        public StateService(IStateRepository stateRepository)
        {
            _stateRepository = stateRepository;
        }

        public async Task<IEnumerable<StateModel>> GetAllStatesAsync()
        {
            return await _stateRepository.GetAllStatesAsync();
        }
    }
}