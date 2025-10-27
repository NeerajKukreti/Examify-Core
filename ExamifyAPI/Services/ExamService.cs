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
        Task<IEnumerable<AvailableQuestionDTO>> GetAvailableQuestionsAsync(int examId, int instituteId);
        Task<IEnumerable<ExamQuestionDTO>> GetExamQuestionsAsync(int examId);
        Task<bool> SaveExamQuestionsAsync(ExamQuestionConfigDTO config);
        Task<bool> RemoveExamQuestionAsync(int examId, int questionId);
        Task<IEnumerable<UserExamDTO>> GetUserExamsAsync(long userId);
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

        public async Task<IEnumerable<AvailableQuestionDTO>> GetAvailableQuestionsAsync(int examId, int instituteId)
        {
            return await _examRepository.GetAvailableQuestionsAsync(examId, instituteId);
        }

        public async Task<IEnumerable<ExamQuestionDTO>> GetExamQuestionsAsync(int examId)
        {
            return await _examRepository.GetExamQuestionsAsync(examId);
        }

        public async Task<bool> SaveExamQuestionsAsync(ExamQuestionConfigDTO config)
        {
            return await _examRepository.SaveExamQuestionsAsync(config);
        }

        public async Task<bool> RemoveExamQuestionAsync(int examId, int questionId)
        {
            return await _examRepository.RemoveExamQuestionAsync(examId, questionId);
        }

        public async Task<IEnumerable<UserExamDTO>> GetUserExamsAsync(long userId)
        {
            return await _examRepository.GetUserExamsAsync(userId);
        }
    }
}
