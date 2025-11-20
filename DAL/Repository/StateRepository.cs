using Dapper;
using DataModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace DAL.Repository
{
    public interface IStateRepository
    {
        Task<List<StateModel>> GetAllStatesAsync();
    }

    public class StateRepository : IStateRepository
    {
        private readonly IConfiguration _config;
        public StateRepository(IConfiguration config) => _config = config;
        private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<List<StateModel>> GetAllStatesAsync()
        {
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<StateModel>(
                "_sp_GetAllStates", 
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }
    }
}