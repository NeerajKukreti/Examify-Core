using DAL.Repository;
using DataModel;
using ExamAPI.Services;

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
        private readonly IAuthService _authService;

        public QuestionExtractorService(IQuestionExtractorRepository repository, IAuthService authService)
        {
            _repository = repository;
            this._authService = authService;
        }

        public async Task<int> SaveExtractedQuestionAsync(QuestionModel model, int instituteId)
        {
            instituteId = _authService.GetCurrentInstituteId();
            return await _repository.SaveExtractedQuestionAsync(model, instituteId);
        }

        public async Task<List<int>> SaveExtractedQuestionsAsync(List<QuestionModel> questions, int instituteId)
        {
            instituteId = _authService.GetCurrentInstituteId();
            return await _repository.SaveExtractedQuestionsAsync(questions, instituteId);
        }
    }
}
