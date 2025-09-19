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
        Task<int> InsertOrUpdateBatchAsync(BatchDTO dto, int? batchId = null, int? createdBy = null, int? modifiedBy = null);
    }

    public class BatchRepository : IBatchRepository
    {
        private readonly IConfiguration _config;
        public BatchRepository(IConfiguration config) => _config = config;

        private IDbConnection Connection => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<BatchModel?> GetBatchByIdAsync(int batchId)
        {
            using var connection = Connection;
            return await connection.QueryFirstOrDefaultAsync<BatchModel>(
                "_sp_GetBatchById",
                new { BatchId = batchId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<BatchModel>> GetBatchesByClassIdAsync(int classId)
        {
            using var connection = Connection;
            return await connection.QueryAsync<BatchModel>(
                "_sp_GetBatchesByClassId",
                new { ClassId = classId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> InsertOrUpdateBatchAsync(BatchDTO dto, int? batchId = null, int? createdBy = null, int? modifiedBy = null)
        {
            using var connection = Connection;
            var parameters = new DynamicParameters();

            parameters.Add("@BatchId", batchId);
            parameters.Add("@ClassId", dto.ClassId);
            parameters.Add("@BatchName", dto.BatchName);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@CreatedBy", createdBy);
            parameters.Add("@ModifiedBy", modifiedBy);

            var newBatchId = await connection.ExecuteScalarAsync<int>(
                "_sp_InsertUpdateBatch",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return newBatchId;
        }
    }
}
