namespace DataModel.Exam
{
    public class ExamResponsePairModel
    {
        public long ResponsePairId { get; set; }
        public long ResponseId { get; set; }
        public string LeftText { get; set; }
        public string RightText { get; set; }
    }
}
