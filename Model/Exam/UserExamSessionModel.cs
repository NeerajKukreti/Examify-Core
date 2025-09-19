using System;
using System.Collections.Generic;

namespace DataModel
{
    public class UserExamSessionModel
    {
        public long UserExamSessionId { get; set; }
        public int ExamId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? SubmitTime { get; set; }
        public string Status { get; set; } = "Started";
        public decimal? TotalScore { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Percentage { get; set; }
        
        // Navigation properties
        public ExamModel? Exam { get; set; }
        public ICollection<ExamSessionQuestionModel>? SessionQuestions { get; set; }
        public ICollection<ExamResponseModel>? ExamResponses { get; set; }
    }
}