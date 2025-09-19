using DAL.Repository;
using DataModel;

namespace ExamifyAPI.Services
{
    public interface IExamService
    {
        List<ExamModel> GetActiveExams();

        ExamModel GetExamById(int examId);
        ExamQuestionsResponse GetExamSessionQuestions(int userId, int examId); 
        int SubmitExamResponses(ExamSubmissionModel submission);
        ExamResultModel GetExamResult(int sessionId);
    }

    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IQuestionRepository _questionRepository;

        public ExamService(IExamRepository examRepository, IQuestionRepository questionRepository)
        {
            _examRepository = examRepository;
            _questionRepository = questionRepository;
        }

        public List<ExamModel> GetActiveExams()
        {
            return _examRepository.GetActiveExams();
        }

        public ExamModel GetExamById(int examId)
        {
            return _examRepository.GetExamById(examId);
        }

        public ExamQuestionsResponse GetExamSessionQuestions(int userId, int examId)
        {
            return _examRepository.GetExamSessionQuestions(userId, examId);
        }

        public int SubmitExamResponses(ExamSubmissionModel submission)
        {
            return _examRepository.SubmitExamResponses(submission);
        }

        public ExamResultModel GetExamResult(int sessionId)
        {
            return _examRepository.GetExamResult(sessionId);
        }
    }
}