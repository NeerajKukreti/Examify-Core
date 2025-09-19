using System.ComponentModel.DataAnnotations;

namespace DataModel
{
    public class UserDTO
    {
        [Required, StringLength(256)]
        public required string Username { get; set; }
        [Required]
        public required string Password { get; set; }

        [Required, StringLength(50)]
        public string? Role { get; set; }
        public int? InstituteId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
