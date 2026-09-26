using DAL.Repository;
using DataModel;
using ExamAPI.Services;
using Model.DTO;

namespace ExamifyAPI.Services
{
    public interface IExamService
    {
        Task<IEnumerable<ExamModel>> GetAllExamsAsync();
        IEnumerable<ExamModel> GetPublicCatalogExams();
        Task<ExamModel> GetExamByIdAsync(int examId);
        Task<ExamModel?> GetSessionExamByIdAsync(int examId);
        Task<int> InsertOrUpdateExamAsync(ExamDTO dto, int? examId = null, int? userloggedIn = null);
        Task<bool> ChangeStatusAsync(int examId);
        Task<bool> PublishExamAsync(int examId);
        int SubmitExamResponses(ExamSubmissionModel submission);
        UserExamSessionModel? GetUserExamSession(long sessionId);
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

        public IEnumerable<ExamModel> GetPublicCatalogExams()
        {
            return _examRepository.GetPublicCatalogExams();
        }

        public async Task<ExamModel> GetExamByIdAsync(int examId)
        {
            var instituteId = _authService.GetCurrentInstituteId();
            var exam = _examRepository.GetExamById(examId, instituteId)
                       ?? _examRepository.GetExamById(examId, 0);
            return exam;
        }

        public async Task<ExamModel?> GetSessionExamByIdAsync(int examId)
        {
            var instituteId = _authService.GetCurrentInstituteId();
            var userId = _authService.GetCurrentUserID();
            var tenantType = _authService.GetCurrentTenantType();
            
            // Check if user has already taken this exam
            var userExams = await _examRepository.GetUserExamsAsync(new List<long> { userId });
            if (userExams.Any(ue => ue.ExamId == examId))
            {
                return null; // User has already taken the exam
            }
            
            var exam = _examRepository.GetExamById(examId, instituteId)
                       ?? _examRepository.GetExamById(examId, 0);

            if (exam == null || exam.IsActive != true || !exam.IsPublished)
            {
                return null;
            }

            // 1. If it's a Public Platform exam:
            if (exam.IsPublic)
            {
                // Enforce monthly quota for Personal tenant users (max 10 free exams / month)
                if (tenantType == 1) // Personal
                {
                    var monthlyAttempts = await _examRepository.GetMonthlyExamCountAsync(userId);
                    if (monthlyAttempts >= 10)
                    {
                        throw new InvalidOperationException("Monthly free exam limit reached (10/10). Upgrade to Pro for unlimited exams.");
                    }
                }
                return exam;
            }
            
            // 2. Otherwise, enforce Institute Class/Batch gating
            var studentClasses = await _classService.GetStudentClassesAsync(userId);
            var studentClassIds = studentClasses.Select(sc => sc.ClassId).ToHashSet();
            
            if (exam.ClassIds.Any(classId => studentClassIds.Contains(classId)))
            {
                return exam;
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
        public UserExamSessionModel? GetUserExamSession(long sessionId)
        {
            return _examRepository.GetUserExamSession(sessionId);
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
