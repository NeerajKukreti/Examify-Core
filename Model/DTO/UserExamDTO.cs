namespace Model.DTO
{
    public class UserExamDTO
    {
        public long UserId { get; set; }
        public int ExamId { get; set; }
        public string? ExamName { get; set; }
        public string? ExamType { get; set; }
        public long UserExamSessionId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? SubmitTime { get; set; }
        public string? Status { get; set; }
        public DateTime? ExamTakenOn { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? CutOffPercentage { get; set; }
    }
}
