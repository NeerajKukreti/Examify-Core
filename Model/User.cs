
using System.ComponentModel.DataAnnotations;

namespace DataModel
{
    public class User 
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Role { get; set; } = ""; //Admin, Institute, Student
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = "";
    }
}
