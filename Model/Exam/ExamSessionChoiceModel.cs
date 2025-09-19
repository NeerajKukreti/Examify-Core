using System;

namespace DataModel
{
    public class ExamSessionChoiceModel
    {
        public long SessionChoiceId { get; set; }
        public long SessionQuestionId { get; set; }
        public int ChoiceId { get; set; }
        public string? ChoiceTextEnglish { get; set; }
        public string? ChoiceTextHindi { get; set; }
        public bool? IsCorrect { get; set; }
        
        // Navigation properties
        public ExamSessionQuestionModel? SessionQuestion { get; set; }
    }
}