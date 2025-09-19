using DAL.Repository;
using DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExamifyAPI.Services
{
    public interface IQuestionService
    {
        Task<List<QuestionModel>> GetAllQuestionsAsync();
        Task<QuestionModel?> GetQuestionByIdAsync(int id);
        Task<int> CreateQuestionAsync(QuestionModel model);
        Task<int> UpdateQuestionAsync(QuestionModel model);
        Task<int> DeleteQuestionAsync(int id);
        Task<List<QuestionTypeModel>> GetQuestionTypesAsync();
    }

    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _repo;
        public QuestionService(IQuestionRepository repo)
        {
            _repo = repo;
        }
        public async Task<List<QuestionModel>> GetAllQuestionsAsync() => await _repo.GetAllQuestionsAsync();
        public async Task<QuestionModel?> GetQuestionByIdAsync(int id) => await _repo.GetQuestionByIdAsync(id);
        public async Task<int> CreateQuestionAsync(QuestionModel model) => await _repo.CreateQuestionAsync(model);
        public async Task<int> UpdateQuestionAsync(QuestionModel model) => await _repo.UpdateQuestionAsync(model);
        public async Task<int> DeleteQuestionAsync(int id) => await _repo.DeleteQuestionAsync(id);
        public async Task<List<QuestionTypeModel>> GetQuestionTypesAsync() => await _repo.GetQuestionTypesAsync();
    }
}
