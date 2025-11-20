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
        Task<IEnumerable<ClassModel>> GetAllClassesAsync(int instituteId, int? classId = null);
        Task<int> InsertOrUpdateClassAsync(ClassDTO dto, int? classId = null, int? createdBy = null);
        Task<bool> ChangeStatus(int classId);
        Task<IEnumerable<StudentClassModel>> GetStudentClassesAsync(int studentId);
    }

    public class ClassRepository : IClassRepository
    {
        private readonly IConfiguration _config;
        public ClassRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<ClassModel>> GetAllClassesAsync(int instituteId, int? classId = null)
        {
            using var connection = CreateConnection();
            
            // Execute the stored procedure that returns multiple result sets
            using var multi = await connection.QueryMultipleAsync(
                "_sp_GetAllClass", 
                new { InstituteId = instituteId, ClassId = classId },
                commandType: CommandType.StoredProcedure
            );

            // Read the first result set (Classes)
            var classes = (await multi.ReadAsync<ClassModel>()).ToList();
            
            // Read the second result set (Batches)
            var batches = (await multi.ReadAsync<BatchModel>()).ToList();

            // Group batches by ClassId and assign to corresponding classes
            var batchGroups = batches.GroupBy(b => b.ClassId).ToDictionary(g => g.Key, g => g.ToList());
            
            foreach (var classModel in classes)
            {
                // Initialize the Batches collection
                classModel.Batches = batchGroups.TryGetValue(classModel.ClassId, out var classBatches) 
                    ? classBatches 
                    : new List<BatchModel>();
            }

            return classes;
        }

         public async Task<int> InsertOrUpdateClassAsync(ClassDTO dto, int? classId = null, int? createdBy = null)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();

            var batchTable = new DataTable();
            batchTable.Columns.Add("BatchId", typeof(int));
            batchTable.Columns.Add("BatchName", typeof(string));
            batchTable.Columns.Add("IsActive", typeof(bool));

            if (dto.Batches != null)
            {
                foreach (var batch in dto.Batches)
                {
                    batchTable.Rows.Add(batch.BatchId > 0 ? (object)batch.BatchId : DBNull.Value, batch.BatchName, batch.IsActive);
                }
            }

            parameters.Add("@ClassId", classId);
            parameters.Add("@InstituteId", dto.InstituteId);
            parameters.Add("@ClassName", dto.ClassName);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@UserId", createdBy);
            parameters.Add("@BatchList", batchTable.AsTableValuedParameter("dbo.BatchTVP"));

            var newClassId = await connection.ExecuteScalarAsync<int>(
                "_sp_InsertUpdateClass",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return newClassId;
        }

        public async Task<bool> ChangeStatus(int classId)
        {
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE Class SET IsActive = 1 - IsActive WHERE classId = @classId",
                new { classId = classId },
                commandType: CommandType.Text
            );
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<StudentClassModel>> GetStudentClassesAsync(int studentId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<StudentClassModel>(
                "_sp_GetStudentClass",
                new { StudentId = studentId },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}


