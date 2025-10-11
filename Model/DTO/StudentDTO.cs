using System.ComponentModel.DataAnnotations;
using DataModel;
namespace Model.DTO
{
    public class StudentDTO
    {
        // Student fields
        public int StudentId { get; set; }

        [Required(ErrorMessage = "InstituteId is required")]
        public int InstituteId { get; set; }

        [Required(ErrorMessage = "Student Name is required")]
        [StringLength(60, ErrorMessage = "Student Name cannot exceed 60 characters")]
        public required string StudentName { get; set; }

        [StringLength(60, ErrorMessage = "Father Name cannot exceed 60 characters")]
        public string? FatherName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(30, ErrorMessage = "Category cannot exceed 30 characters")]
        public string? Category { get; set; }

        public int? StateId { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [StringLength(60, ErrorMessage = "City cannot exceed 60 characters")]
        public string? City { get; set; }

        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string? Pincode { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [Phone(ErrorMessage = "Invalid Mobile number")]
        [StringLength(10, ErrorMessage = "Mobile number must be 10 digits")]
        public required string Mobile { get; set; }   // will be username

        [StringLength(10, ErrorMessage = "Secondary Contact cannot exceed 10 digits")]
        public string? SecondaryContact { get; set; }

        [StringLength(10, ErrorMessage = "Parent Contact cannot exceed 10 digits")]
        public string? ParentContact { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(256)]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ActivationDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Validity { get; set; }

        public bool IsActive { get; set; }

        // User info for creation
        [Required(ErrorMessage = "Username is required")]
        [StringLength(256, ErrorMessage = "Username cannot exceed 256 characters")]
        public required string UserName { get; set; }   // same as Mobile
         
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string? Password { get; set; }

        public int UserId { get; set; } // Assigned after user creation
        public int? BatchId { get; set; }

        // Lookup lists for UI (existing models/DTOs)
        public List<string>? Categories { get; set; }             // from config
        public List<StateModel>? States { get; set; }            // from State table
        public List<ClassDTO>? Classes { get; set; }             // from Class table for this institute
        public List<BatchDTO>? Batches { get; set; }             // batches under selected class
    }
}
