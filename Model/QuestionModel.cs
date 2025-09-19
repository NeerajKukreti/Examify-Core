namespace DataModel
{
    public class QuestionModel
    {
        public int QuestionId { get; set; }
        public int TopicId { get; set; }
        public int SubjectId { get; set; }
        public string TopicName { get; set; }
        public string QuestionEnglish { get; set; } = string.Empty;
        public string QuestionHindi { get; set; } = string.Empty;
        public string AdditionalTextEnglish { get; set; } = string.Empty;
        public string AdditionalTextHindi { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsMultiSelect { get; set; }
    }
}