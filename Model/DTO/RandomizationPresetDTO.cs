using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class RandomizationPresetDTO
    {
        public int PresetId { get; set; }
        
        [Required, MaxLength(100)]
        public string PresetName { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public int InstituteId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
        
        public List<RandomizationPresetDetailDTO> Details { get; set; } = new();
    }

    public class RandomizationPresetDetailDTO
    {
        public int PresetDetailId { get; set; }
        public int PresetId { get; set; }
        public int? SubjectId { get; set; }
        public int? TopicId { get; set; }
        public string? DifficultyLevel { get; set; }
        public int? QuestionTypeId { get; set; }
        public int PickCount { get; set; }
        
        // Display
        public string? SubjectName { get; set; }
        public string? TopicName { get; set; }
        public string? QuestionTypeName { get; set; }
    }

    public class PresetExecutionDTO
    {
        public int ExamId { get; set; }
        public int PresetId { get; set; }
        public decimal DefaultMarks { get; set; } = 1.0m;
        public decimal DefaultNegativeMarks { get; set; } = 0.0m;
    }

    public class PresetPreviewDTO
    {
        public int TotalQuestions { get; set; }
        public List<PresetPreviewDetailDTO> Details { get; set; } = new();
    }

    public class PresetPreviewDetailDTO
    {
        public string? SubjectName { get; set; }
        public string? TopicName { get; set; }
        public string? DifficultyLevel { get; set; }
        public int RequestedCount { get; set; }
        public int AvailableCount { get; set; }
        public bool HasEnough { get; set; }
    }
}
