using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class ExamQuestionDTO
    {
        public int ExamId { get; set; }
        public int QuestionId { get; set; }
        
        [Range(0.01, 100, ErrorMessage = "Marks must be between 0.01 and 100")]
        public decimal Marks { get; set; } = 1.0m;
        
        [Range(0, 100, ErrorMessage = "Negative marks must be between 0 and 100")]
        public decimal NegativeMarks { get; set; } = 0.0m;
        
        public int? SortOrder { get; set; }
        
        // For display purposes
        public string? QuestionEnglish { get; set; }
        public string? TopicName { get; set; }
        public string? QuestionTypeName { get; set; }
    }

    public class ExamQuestionConfigDTO
    {
        public int ExamId { get; set; }
        public List<ExamQuestionDTO> Questions { get; set; } = new();
    }

    public class AvailableQuestionDTO
    {
        public int QuestionId { get; set; }
        public string? QuestionEnglish { get; set; }
        public string? QuestionHindi { get; set; }
        public string? TopicName { get; set; }
        public string? SubjectName { get; set; }
        public int SubjectId { get; set; }
        public string? QuestionTypeName { get; set; }
        public string? DifficultyLevel { get; set; }
        public bool IsMultiSelect { get; set; }
    }
}
