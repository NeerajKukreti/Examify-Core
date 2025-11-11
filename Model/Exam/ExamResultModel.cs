using DataModel.Exam;
using System;
using System.Collections.Generic;

namespace DataModel
{
    public class ExamResultModel
    {
        public int SessionId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public decimal TotalMarks { get; set; }
        public decimal ObtainedMarks { get; set; }
        public decimal Percentage { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int UnattemptedQuestions { get; set; }
        public int TotalTimeSpent { get; set; }
        public DateTime SubmittedAt { get; set; }
        public decimal? CutOffPercentage { get; set; }
        public List<QuestionResultModel> QuestionResults { get; set; } = new List<QuestionResultModel>();
        public List<ExamResponsePairModel> ResponsePairs { get; set; } = new List<ExamResponsePairModel>();
        public List<ExamResponseOrderModel> ResponseOrders { get; set; } = new List<ExamResponseOrderModel>();
    }

    public class QuestionResultModel
    { 
        public long QuestionId { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
        public long? SessionChoiceId { get; set; } 
        public string SelectedChoiceText { get; set; } = string.Empty;
        public int CorrectChoiceId { get; set; }
        public string CorrectChoiceText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public bool IsAttempted { get; set; }
        public decimal Marks { get; set; }
        public decimal MarksAwarded { get; set; }
        public int TimeSpent { get; set; }
        public decimal NegativeMarks { get; set; }
        public bool IsMultiSelect { get; set; }
        public string ResponseText { get; set; } = string.Empty;
        public dynamic AllChoices { get; set; }
        public dynamic CorrectChoices { get; set; }
        public List<ExamResponsePairModel> ResponsePairs { get; set; } = new List<ExamResponsePairModel>();
        public List<ExamResponseOrderModel> ResponseOrders { get; set; } = new List<ExamResponseOrderModel>();
    }
}