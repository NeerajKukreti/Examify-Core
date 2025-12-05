using DAL.Repository;
using DataModel;

namespace ExamifyAPI.Services
{
    public interface IQuestionExtractorService
    {
        Task<int> SaveExtractedQuestionAsync(QuestionModel model, int instituteId);
        Task<List<int>> SaveExtractedQuestionsAsync(List<QuestionModel> questions, int instituteId);
    }

    public class QuestionExtractorService : IQuestionExtractorService
    {
        private readonly IQuestionExtractorRepository _repository;

        public QuestionExtractorService(IQuestionExtractorRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> SaveExtractedQuestionAsync(QuestionModel model, int instituteId)
        {
            return await _repository.SaveExtractedQuestionAsync(model, instituteId);
        }

        public async Task<List<int>> SaveExtractedQuestionsAsync(List<QuestionModel> questions, int instituteId)
        {
            return await _repository.SaveExtractedQuestionsAsync(questions, instituteId);
        }
    }
}
