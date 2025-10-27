using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class ExamDTO
    {
        public int ExamId { get; set; }

        [Required(ErrorMessage = "Exam Name is required")]
        [StringLength(200, ErrorMessage = "Exam Name cannot exceed 200 characters")]
        public required string ExamName { get; set; }

        [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Image path cannot exceed 200 characters")]
        public string? Image { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes")]
        public int DurationMinutes { get; set; } = 60;

        //[Required(ErrorMessage = "Total Questions is required")]
        //[Range(1, 500, ErrorMessage = "Total Questions must be between 1 and 500")]
        public int TotalQuestions { get; set; }

        [StringLength(4000, ErrorMessage = "Instructions cannot exceed 4000 characters")]
        public string? Instructions { get; set; }

        [StringLength(50, ErrorMessage = "Exam Type cannot exceed 50 characters")]
        public string? ExamType { get; set; }

        [Range(0, 100, ErrorMessage = "Cut Off Percentage must be between 0 and 100")]
        public decimal? CutOffPercentage { get; set; }
    }
}
