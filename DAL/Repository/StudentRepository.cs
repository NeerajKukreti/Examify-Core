using Dapper;
using DataModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model.DTO;
using System.Data;

namespace DAL.Repository
{
    public interface IStudentRepository
    {
        Task<int> InsertOrUpdateStudentAsync(StudentDTO dto, int? studentId = null, int? createdBy = null, int? modifiedBy = null);
        Task<int> InsertStudentClassAsync(int studentId, int classId, int createdBy);
        Task<int> InsertStudentBatchAsync(int studentId, int batchId, int createdBy, DateTime? enrollmentDate = null);
        Task<IEnumerable<StudentModel>> GetAllStudentsAsync(int instituteId, int? studentId);
        Task<bool> ChangeStatus(int studentId);
    }

    public class StudentRepository : IStudentRepository
    {
        private readonly IConfiguration _config;
        public StudentRepository(IConfiguration config) => _config = config;

        private IDbConnection Connection => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<int> InsertOrUpdateStudentAsync(StudentDTO dto, int? studentId = null, int? createdBy = null, int? modifiedBy = null)
        {
            using var connection = Connection;
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);
            parameters.Add("@StudentName", dto.StudentName);
            parameters.Add("@FatherName", dto.FatherName);
            parameters.Add("@DateOfBirth", dto.DateOfBirth);
            parameters.Add("@Category", dto.Category);
            parameters.Add("@StateId", dto.StateId);
            parameters.Add("@Address", dto.Address);
            parameters.Add("@City", dto.City);
            parameters.Add("@Pincode", dto.Pincode);
            parameters.Add("@Mobile", dto.Mobile);
            parameters.Add("@SecondaryContact", dto.SecondaryContact);
            parameters.Add("@ParentContact", dto.ParentContact);
            parameters.Add("@Email", dto.Email);
            parameters.Add("@ActivationDate", dto.ActivationDate);
            parameters.Add("@Validity", dto.Validity); 
            parameters.Add("@UserId", dto.UserId);
            parameters.Add("@InstituteId", dto.InstituteId);
            parameters.Add("@CreatedBy", createdBy);
            parameters.Add("@ModifiedBy", modifiedBy);

            return await connection.ExecuteScalarAsync<int>(
                "_sp_InsertUpdateStudent",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> InsertStudentClassAsync(int studentId, int classId, int createdBy)
        {
            using var connection = Connection;
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);
            parameters.Add("@ClassId", classId);
            parameters.Add("@CreatedBy", createdBy);

            return await connection.ExecuteScalarAsync<int>(
                "_sp_InsertStudentClass",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> InsertStudentBatchAsync(int studentId, int batchId, int createdBy, DateTime? enrollmentDate = null)
        {
            using var connection = Connection;
            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);
            parameters.Add("@BatchId", batchId);
            parameters.Add("@CreatedBy", createdBy);
            parameters.Add("@EnrollmentDate", enrollmentDate);

            return await connection.ExecuteScalarAsync<int>(
                "_sp_InsertStudentBatch",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<StudentModel>> GetAllStudentsAsync(int instituteId, int? studentId)
        {
            using var connection = Connection;
            return await connection.QueryAsync<StudentModel>(
                "_sp_GetAllStudents",
                new { InstituteId = instituteId, StudentId = studentId },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<bool> ChangeStatus(int studentId)
        {
            using var connection = Connection;
            // Toggle IsActive for the given studentId
            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE Student SET IsActive = ~IsActive WHERE StudentId = @StudentId",
                new { StudentId = studentId },
                commandType: CommandType.Text
            );

            // Return true if at least one row was updated
            return rowsAffected > 0;
        }


    }
}
