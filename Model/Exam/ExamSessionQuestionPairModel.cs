namespace DataModel.Exam
{
    public class ExamSessionQuestionPairModel
    {
        public long SessionPairId { get; set; }
        public long SessionQuestionId { get; set; }
        public string LeftText { get; set; }
        public string RightText { get; set; }
    }
}
