namespace OnlineExam.Common
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public ApiEndpoints Endpoints { get; set; } = new();
        public string StartExamUrl { get; set; } = string.Empty;
        public string ExamResultUrl { get; set; } = string.Empty;
    }

    public class ApiEndpoints
    {
        public string Login { get; set; } = string.Empty;
        public string ExamList { get; set; } = string.Empty;
        public string ExamById { get; set; } = string.Empty;
        public string ExamQuestions { get; set; } = string.Empty;
        public string ExamSubmit { get; set; } = string.Empty;
        // QuestionBank endpoints
        public string QuestionBankList { get; set; } = string.Empty;
        public string QuestionBankById { get; set; } = string.Empty;
        public string QuestionBankCreate { get; set; } = string.Empty;
        public string QuestionBankUpdate { get; set; } = string.Empty;
        public string QuestionBankDelete { get; set; } = string.Empty;
    }
}