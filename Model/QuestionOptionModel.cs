using System;
using System.Collections.Generic; 

namespace DataModel
{
    public class QuestionOptionModel
    {
        public int Order { get; set; }
        public int QuestionId { get; set; }
        public int QuestionChoiceId { get; set; }
        public string ChoiceTextEnglish { get; set; }
        public string ChoiceTextHindi { get; set; }
        public bool IsCorrect { get; set; }
    }
}
