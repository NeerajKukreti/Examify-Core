namespace OnlineExam.Common
{
    public static class Constants
    {
        // Base API URL and client name
        public const string BaseUrl = "https://localhost:7271/api/";
        public const string ClientName = "ExamifyAPI";

        // Exam endpoints
        public const string StartExamUrl = "/Exam/StartExam";
        public const string ExamResultUrl = "/Exam/ExamResult";
        public const string Login = "auth/login";
        public const string ExamList = "exam/list";
        public const string ExamById = "exam/GetExamById";
        public const string ExamQuestions = "Exam/{0}/sessionquestions";
        public const string ExamSubmit = "Exam/{0}/submit";

        // Question endpoints
        public const string QuestionList = "question/list";
        public const string QuestionById = "question/GetQuestionById";
        public const string QuestionCreate = "question/create";
        public const string QuestionUpdate = "question/update";
        public const string QuestionDelete = "question/delete/{0}";

        // Add other endpoints as needed
    }
}
