using System;

namespace DataModel
{
    public class ExamResponseModel
    {
        public long ResponseId { get; set; }
        public long UserExamSessionId { get; set; }
        public long SessionQuestionId { get; set; }
        public long? SessionChoiceId { get; set; }
        public int? TimeSpent { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ResponseText { get; set; } // Added for subjective/pairing questions

        // Navigation properties
        public UserExamSessionModel? UserExamSession { get; set; }
        public ExamSessionQuestionModel? SessionQuestion { get; set; }
        public ExamSessionChoiceModel? SessionChoice { get; set; }
    }
}