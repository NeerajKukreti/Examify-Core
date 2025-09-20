using DAL.Repository;
using DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineExam.Services
{
    public interface IQuestionService
    {
        Task<List<QuestionModel>> GetAllQuestionsAsync();
        Task<QuestionModel?> GetQuestionAsync(int id);
        Task<int> CreateQuestionAsync(QuestionModel model);
        Task<int> UpdateQuestionAsync(QuestionModel model);
        Task<int> DeleteQuestionAsync(int id);
    }

    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _repo;
        public QuestionService(IQuestionRepository repo)
        {
            _repo = repo;
        }
        public Task<List<QuestionModel>> GetAllQuestionsAsync() => _repo.GetAllQuestionsAsync();
        public Task<QuestionModel?> GetQuestionAsync(int id) => _repo.GetQuestionAsync(id);
        public Task<int> CreateQuestionAsync(QuestionModel model) => _repo.CreateQuestionAsync(model);
        public Task<int> UpdateQuestionAsync(QuestionModel model) => _repo.UpdateQuestionAsync(model);
        public Task<int> DeleteQuestionAsync(int id) => _repo.DeleteQuestionAsync(id);
    }
}
