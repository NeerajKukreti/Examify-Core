using Microsoft.Data.SqlClient;
using Dapper;
using DataModel;
using Microsoft.Extensions.Configuration;

using System.Data;

namespace DAL
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsername(string username);
        Task<int> CreateUser(User user);
        Task<bool> CheckUserNameExistsAsync(string userName, int? userId = null);
    }
    // Repositories/UserRepository.cs

    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _config;
        public UserRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<User?> GetUserByUsername(string username)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);

            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(
                "_sp_GetUser",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateUser(User user)
        {
            using var conn = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserName", user.Username);
            parameters.Add("@PasswordHash", user.PasswordHash);
            parameters.Add("@Role", user.Role);

            // call stored procedure
            var userId = await conn.ExecuteScalarAsync<int>(
                "_sp_CreateUser",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return userId;
        }

        public async Task<bool> CheckUserNameExistsAsync(string userName, int? userId = null)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@UserName", userName);
            parameters.Add("@UserId", userId);

            var result = await connection.QuerySingleAsync<bool>(
                "_sp_CheckUserNameExists",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
    }

}