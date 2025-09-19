using Dapper;
using DataModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model.DTO;
using System.Data;

namespace DAL.Repository
{
    public interface IClassRepository
    {
        Task<IEnumerable<ClassModel>> GetAllClassesAsync(int instituteId);
        Task<ClassModel?> GetClassByIdAsync(int classId);
        Task<int> InsertOrUpdateClassAsync(ClassDTO dto, int? classId = null, int? createdBy = null, int? modifiedBy = null);
    }

    public class ClassRepository : IClassRepository
    {
        private readonly IConfiguration _config;
        public ClassRepository(IConfiguration config) => _config = config;

        private IDbConnection Connection => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<ClassModel>> GetAllClassesAsync(int instituteId)
        {
            using var connection = Connection;
            return await connection.QueryAsync<ClassModel>(
                "_sp_GetAllClass", new { InstituteId = instituteId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<ClassModel?> GetClassByIdAsync(int classId)
        {
            using var connection = Connection;
            return await connection.QueryFirstOrDefaultAsync<ClassModel>(
                "_sp_GetClassById",
                new { ClassId = classId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> InsertOrUpdateClassAsync(ClassDTO dto, int? classId = null, int? createdBy = null, int? modifiedBy = null)
        {
            using var connection = Connection;
            var parameters = new DynamicParameters();

            parameters.Add("@ClassId", classId);
            parameters.Add("@InstituteId", dto.InstituteId);
            parameters.Add("@ClassName", dto.ClassName);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@CreatedBy", createdBy);
            parameters.Add("@ModifiedBy", modifiedBy);

            var newClassId = await connection.ExecuteScalarAsync<int>(
                "_sp_InsertUpdateClass",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return newClassId;
        }
    }
}
