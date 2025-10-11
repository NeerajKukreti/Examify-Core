using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class ClassDTO
    {
        public int ClassId { get; set; }

        [Required(ErrorMessage = "InstituteId is required")]
        public int InstituteId { get; set; }

        [Required(ErrorMessage = "Class Name is required")]
        [StringLength(50, ErrorMessage = "Class Name cannot exceed 50 characters")]
        public string ClassName { get; set; }

        public bool? IsActive { get; set; }

        // Batches collection for class creation/editing
        public List<BatchDTO>? Batches { get; set; }
    }
}
