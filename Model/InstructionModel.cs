namespace DataModel
{
    public class ExamInstructionModel
    {
        public int? InstructionId { get; set; }
        public int InstituteId { get; set; }
        public string InstructionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
