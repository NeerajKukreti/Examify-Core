namespace DataModel
{
    public class QuestionModel
    {
        public int? QuestionId { get; set; } = 0;
        public int TopicId { get; set; }
        public int SubjectId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string QuestionEnglish { get; set; } = string.Empty;
        public string QuestionHindi { get; set; } = string.Empty;
        public string AdditionalTextEnglish { get; set; } = string.Empty;
        public string AdditionalTextHindi { get; set; } = string.Empty;
        public string? Explanation { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsMultiSelect { get; set; }
        public List<OptionModel> Options { get; set; }
    }

    public class OptionModel
    {
        public int? ChoiceId { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}