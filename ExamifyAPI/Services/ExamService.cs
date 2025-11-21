using DAL.Repository;
using DataModel;
using ExamAPI.Services;
using Model.DTO;

namespace ExamifyAPI.Services
{
    public interface IExamService
    {
        Task<IEnumerable<ExamModel>> GetAllExamsAsync();
        Task<ExamModel> GetExamByIdAsync(int examId);
        Task<int> InsertOrUpdateExamAsync(ExamDTO dto, int? examId = null, int? userloggedIn = null);
        Task<bool> ChangeStatusAsync(int examId);
        Task<bool> PublishExamAsync(int examId);
        int SubmitExamResponses(ExamSubmissionModel submission);
        ExamQuestionsResponse GetExamSessionQuestions(int userId, int examId);
        ExamResultModel GetExamResult(int sessionId);
        Task<IEnumerable<AvailableQuestionDTO>> GetAvailableQuestionsAsync(int examId, int instituteId);
        Task<IEnumerable<ExamQuestionDTO>> GetExamQuestionsAsync(int examId);
        Task<bool> SaveExamQuestionsAsync(ExamQuestionConfigDTO config);
        Task<bool> RemoveExamQuestionAsync(int examId, int questionId);
        Task<IEnumerable<UserExamDTO>> GetUserExamsAsync(List<long> userIds);
        Task<StatsDTO> GetStatsAsync();
        Task<IEnumerable<ExamInstructionModel>> GetInstructionsAsync(int instituteId);
        Task<int> UpsertInstructionAsync(ExamInstructionModel model);
    }

    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IAuthService _authService;
        private readonly IClassService _classService;

        public ExamService(IExamRepository examRepository, IAuthService authService, IClassService classService)
        {
            _examRepository = examRepository;
            _authService = authService;
            _classService = classService;
        }

        public async Task<IEnumerable<ExamModel>> GetAllExamsAsync()
        {
            var instituteId = _authService.GetCurrentInstituteId();
            return await Task.FromResult(_examRepository.GetActiveExams(instituteId));
        }

        public async Task<ExamModel> GetExamByIdAsync(int examId)
        {
            var instituteId = _authService.GetCurrentInstituteId();
            var userId = _authService.GetCurrentUserID();
            
            // Check if user has already taken this exam
            var userExams = await _examRepository.GetUserExamsAsync(new List<long> { userId });
            if (userExams.Any(ue => ue.ExamId == examId))
            {
                return null; // User has already taken the exam
            }
            
            var exam = _examRepository.GetExamById(examId, instituteId);
            var studentClasses = await _classService.GetStudentClassesAsync(userId);
            var studentClassIds = studentClasses.Select(sc => sc.ClassId).ToHashSet();
            
            if (exam != null && exam.ClassIds.Any(classId => studentClassIds.Contains(classId)))
            {
                return await Task.FromResult(exam);
            }
            
            return null;
        }

        public async Task<int> InsertOrUpdateExamAsync(ExamDTO dto, int? examId = null, int? userloggedIn = null)
        {
            var instituteId = _authService.GetCurrentInstituteId();
            return await _examRepository.InsertOrUpdateExamAsync(dto, examId, userloggedIn, instituteId);
        }

        public async Task<bool> ChangeStatusAsync(int examId)
        {
            return await _examRepository.ChangeStatus(examId);
        }

        public async Task<bool> PublishExamAsync(int examId)
        {
            return await _examRepository.PublishExam(examId);
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

        public async Task<IEnumerable<UserExamDTO>> GetUserExamsAsync(List<long> userIds)
        {
            return await _examRepository.GetUserExamsAsync(userIds);
        }

        public async Task<StatsDTO> GetStatsAsync()
        {
            var instituteId = _authService.GetCurrentInstituteId();
            return await _examRepository.GetStatsAsync(instituteId);
        }

        public async Task<IEnumerable<ExamInstructionModel>> GetInstructionsAsync(int instituteId)
        {
            return await _examRepository.GetInstructionsAsync(instituteId);
        }

        public async Task<int> UpsertInstructionAsync(ExamInstructionModel model)
        {
            return await _examRepository.UpsertInstructionAsync(model);
        }
    }
}
