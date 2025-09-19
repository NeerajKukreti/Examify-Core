namespace DataModel.Exam
{
    public class ExamSessionQuestionOrderModel
    {
        public long SessionOrderId { get; set; }
        public long SessionQuestionId { get; set; }
        public string ItemText { get; set; }
        public int CorrectOrder { get; set; }
    }
}
