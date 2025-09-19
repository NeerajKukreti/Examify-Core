namespace DataModel
{
    public class BatchModel
    {
        public int BatchId { get; set; }
        public int ClassId { get; set; }
        public string BatchName { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public ClassModel Class { get; set; }
    }
}
