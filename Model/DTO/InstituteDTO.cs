using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class InstituteDTO
    {
        public int InstituteId { get; set; }

        [Required(ErrorMessage = "Institute Name is required")]
        [StringLength(200, ErrorMessage = "Institute Name cannot exceed 200 characters")]
        public required string InstituteName { get; set; }

        [StringLength(50, ErrorMessage = "Short Name cannot exceed 50 characters")]
        public string ShortName { get; set; }

        [StringLength(50, ErrorMessage = "Logo path cannot exceed 50 characters")]
        public string Logo { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string City { get; set; }

        public int? StateId { get; set; }

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string Pincode { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(12, ErrorMessage = "Phone number cannot exceed 12 digits")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(256)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Primary Contact is required")]
        [StringLength(10, ErrorMessage = "Primary Contact cannot exceed 10 digits")]
        public required string PrimaryContact { get; set; }

        [StringLength(10, ErrorMessage = "Secondary Contact cannot exceed 10 digits")]
        public string SecondaryContact { get; set; }

        public DateTime? ActivationDate { get; set; }
        public DateTime? Validity { get; set; }
        public bool? IsActive { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string? Password { get; set; }  // optional: only needed during creation
    }
}
