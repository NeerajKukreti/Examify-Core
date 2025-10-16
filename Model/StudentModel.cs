namespace DataModel
{
    public class StudentModel
    {
        public int StudentId { get; set; }
        public int InstituteId { get; set; }
        public int BatchId { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Category { get; set; }
        public int? StateId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Pincode { get; set; }
        public string Mobile { get; set; }
        public string SecondaryContact { get; set; }
        public string ParentContact { get; set; }
        public string Email { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? Validity { get; set; }
        public bool? IsActive { get; set; }
        public int UserId { get; set; }

        // Audit fields
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public User User { get; set; }
        public StateModel State { get; set; }
        public InstituteModel Institute { get; set; }
        public ICollection<StudentBatch> StudentBatches { get; set; } = new List<StudentBatch>();
    }

    public class StudentBatch
    {
        public int StudentBatchId { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public int BatchId { get; set; }
        public bool? IsActive { get; set; }
         
        public DateTime? EnrollmentDate { get; set; }
    }
}
