namespace DataModel
{
    public class ClassModel
    {
        public int ClassId { get; set; }
        public int InstituteId { get; set; }
        public string ClassName { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public InstituteModel Institute { get; set; }
        public ICollection<BatchModel> Batches { get; set; }
    }
}
