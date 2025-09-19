namespace DataModel
{
    public class QuestionTypeModel
    {
        public int QuestionTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public bool? IsObjective { get; set; }
    }
}
