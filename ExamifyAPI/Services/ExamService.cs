using DAL.Repository;
using DataModel;
using Model.DTO;

namespace ExamifyAPI.Services
{
    public interface IExamService
    {
        Task<IEnumerable<ExamModel>> GetAllExamsAsync();
        Task<ExamModel> GetExamByIdAsync(int examId);
        Task<int> InsertOrUpdateExamAsync(ExamDTO dto, int? examId = null, int? userloggedIn = null);
        Task<bool> ChangeStatusAsync(int examId);
        int SubmitExamResponses(ExamSubmissionModel submission);
        ExamQuestionsResponse GetExamSessionQuestions(int userId, int examId);
        ExamResultModel GetExamResult(int sessionId);
    }

    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;

        public ExamService(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task<IEnumerable<ExamModel>> GetAllExamsAsync()
        {
            return await Task.FromResult(_examRepository.GetActiveExams());
        }

        public async Task<ExamModel> GetExamByIdAsync(int examId)
        {
            return await Task.FromResult(_examRepository.GetExamById(examId));
        }

        public async Task<int> InsertOrUpdateExamAsync(ExamDTO dto, int? examId = null, int? userloggedIn = null)
        {
            return await _examRepository.InsertOrUpdateExamAsync(dto, examId, userloggedIn);
        }

        public async Task<bool> ChangeStatusAsync(int examId)
        {
            return await _examRepository.ChangeStatus(examId);
        }
        public int SubmitExamResponses(ExamSubmissionModel submission)
        {
            return _examRepository.SubmitExamResponses(submission);
        }
        public ExamQuestionsResponse GetExamSessionQuestions(int userId, int examId)
        {
            return _examRepository.GetExamSessionQuestions(userId, examId);
        }

        public ExamResultModel GetExamResult(int sessionId)
        {
            return _examRepository.GetExamResult(sessionId);
        }
    }
}
