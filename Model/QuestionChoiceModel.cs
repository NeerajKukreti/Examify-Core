using System;

namespace DataModel
{
    public class QuestionChoiceModel
    {
        public int QuestionId { get; set; }
        public int ChoiceId { get; set; }
        public string ChoiceTextEnglish { get; set; } = string.Empty;
        public string ChoiceTextHindi { get; set; } = string.Empty;
        public bool? IsCorrect { get; set; }
        
        // Navigation property
        //public QuestionModel? Question { get; set; }
    }
}