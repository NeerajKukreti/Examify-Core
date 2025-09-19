using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class BatchDTO
    {
        public int BatchId { get; set; }

        [Required(ErrorMessage = "ClassId is required")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Batch Name is required")]
        [StringLength(50, ErrorMessage = "Batch Name cannot exceed 50 characters")]
        public string BatchName { get; set; }

        public bool? IsActive { get; set; }
    }
}
