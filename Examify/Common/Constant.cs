namespace Examify.Common.constants
{
    public static class ENDPOINTS
    {
        // Base API URL and client name
        public const string BaseUrl = "https://localhost:7271/api/";
        public const string ClientName = "ExamifyAPI";

        // Exam endpoints
        public const string StartExamUrl = "/ExamSession/StartExam";
        public const string ExamResultUrl = "/ExamSession/ExamResult";
        public const string Login = "auth/login";
        public const string ExamList = "ExamSession/list";
        public const string ExamById = "Exam"; //Api
        public const string ExamQuestions = "Exam/{0}/sessionquestions";
        public const string ExamSubmit = "Exam/{0}/submit";

        // Question endpoints
        public const string QuestionList = "question/list";
        public const string QuestionById = "question/GetQuestion";
        public const string QuestionCreate = "question";
        public const string QuestionUpdate = "question/update";
        public const string QuestionDelete = "question/delete/{0}";

        // Add other endpoints as needed
    }

    public static class Question
    {
        public const string QuestionUploads = "QuesionUploads"; 
    }
}
