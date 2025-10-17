using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class SubjectDTO
    {
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Institute ID is required")]
        public int InstituteId { get; set; }

        [Required(ErrorMessage = "Subject Name is required")]
        [StringLength(200, ErrorMessage = "Subject Name cannot exceed 200 characters")]
        public required string SubjectName { get; set; }

        [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessage = "Image path cannot exceed 200 characters")]
        public string? Image { get; set; }

        public bool IsActive { get; set; } = true;

        public List<SubjectTopicDTO>? Topics { get; set; }
    }

    public class SubjectTopicDTO
    {
        public int TopicId { get; set; }
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Topic Name is required")]
        [StringLength(80, ErrorMessage = "Topic Name cannot exceed 80 characters")]
        public required string TopicName { get; set; }

        [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
