namespace DataModel
{
    public class InstituteModel
    {
        public int InstituteId { get; set; }
        public string InstituteName { get; set; }
        public string ShortName { get; set; }
        public string Logo { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public int? StateId { get; set; }
        public string Pincode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PrimaryContact { get; set; }
        public string SecondaryContact { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? Validity { get; set; }
        public bool? IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int UserId { get; set; }
    }
}
