using DAL.Repository;
using DataModel;
using ExamAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExamifyAPI.Services
{
    public interface IQuestionService
    {
        Task<List<QuestionModel>> GetAllQuestionsAsync();
        Task<QuestionModel?> GetQuestionAsync(int id);
        Task<int> CreateQuestionAsync(QuestionModel model);
        Task<int> UpdateQuestionAsync(QuestionModel model);
        Task<int> DeleteQuestionAsync(int id);
        Task<List<QuestionTypeModel>> GetQuestionTypesAsync();
    }

    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _repo;
        private readonly IAuthService _authService;
        public QuestionService(IQuestionRepository repo, IAuthService authService)
        {
            _repo = repo;
            _authService = authService;
        }
        public async Task<List<QuestionModel>> GetAllQuestionsAsync()
        {
            var instituteId = _authService.GetCurrentInstituteId();
            return await _repo.GetAllQuestionsAsync(instituteId);
        }
        public async Task<QuestionModel?> GetQuestionAsync(int id)
        {
            var instituteId = _authService.GetCurrentInstituteId();
            return await _repo.GetQuestionAsync(id, instituteId);
        }
        public async Task<int> CreateQuestionAsync(QuestionModel model)
        {
            var instituteId = _authService.GetCurrentInstituteId();
            return await _repo.CreateQuestionAsync(model, instituteId);
        }
        public async Task<int> UpdateQuestionAsync(QuestionModel model) => await _repo.UpdateQuestionAsync(model);
        public async Task<int> DeleteQuestionAsync(int id) => await _repo.DeleteQuestionAsync(id);
        public async Task<List<QuestionTypeModel>> GetQuestionTypesAsync() => await _repo.GetQuestionTypesAsync();
    }
}
