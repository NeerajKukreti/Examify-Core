namespace DataModel.Exam
{
    public class ExamResponseOrderModel
    {
        public long ResponseOrderId { get; set; }
        public long SessionQuestionId  { get; set; }
        public long ResponseId { get; set; }
        public string ItemText { get; set; }
        public int UserOrder { get; set; }
        public int CorrectOrder { get; set; }  
    }
}
