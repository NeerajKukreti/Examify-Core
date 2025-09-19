using System;

namespace DataModel
{
    public class ExamModel
    {
        public int ExamId { get; set; }
        public string? ExamName { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool? IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public int TotalQuestions { get; set; }
        public string? Instructions { get; set; }
        public string? ExamType { get; set; }
        public decimal? CutOffPercentage { get; set; }
    }
}