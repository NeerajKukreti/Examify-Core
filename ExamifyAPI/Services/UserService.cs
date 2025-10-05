using DAL;
using DataModel;

namespace ExamifyAPI.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<int> CreateUserAsync(string username, string password, string role);
        Task<bool> CheckUserNameExistsAsync(string userName, int? userId = null);
        Task<bool> UpdateUserAsync(int userId, User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<User?> GetUserByIdAsync(int userId);
        bool VerifyPassword(string password, string hashedPassword);
        string HashPassword(string password);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetUserByUsername(username);
        }

        public async Task<int> CreateUserAsync(string username, string password, string role)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User 
            { 
                Username = username, 
                PasswordHash = hashedPassword, 
                Role = role,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            
            return await _userRepository.CreateUser(user);
        }

        public async Task<bool> CheckUserNameExistsAsync(string userName, int? userId = null)
        {
            return await _userRepository.CheckUserNameExistsAsync(userName, userId);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            // This method would need to be added to IUserRepository if not already present
            // For now, return null as placeholder
            return null;
        }

        public async Task<bool> UpdateUserAsync(int userId, User user)
        {
            // This method would need to be added to IUserRepository if not already present
            // Implementation would update user details
            return false;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            // This method would need to be added to IUserRepository if not already present
            // Implementation would soft delete or hard delete user
            return false;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}