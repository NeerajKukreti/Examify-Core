using Dapper;
using DataModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model.DTO;
using System.Data;

namespace DAL.Repository
{
    public interface IBatchRepository
    {
        Task<BatchModel?> GetBatchByIdAsync(int batchId);
        Task<IEnumerable<BatchModel>> GetBatchesByClassIdAsync(int classId);
    }

    public class BatchRepository : IBatchRepository
    {
        private readonly IConfiguration _config;
        public BatchRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<BatchModel?> GetBatchByIdAsync(int batchId)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<BatchModel>(
                "_sp_GetBatchById",
                new { BatchId = batchId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<BatchModel>> GetBatchesByClassIdAsync(int classId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<BatchModel>(
                "_sp_GetBatchesByClassId",
                new { ClassId = classId },
                commandType: CommandType.StoredProcedure
            );
        }

        
    }
}
