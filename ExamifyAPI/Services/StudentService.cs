using DAL.Repository;
using DataModel;
using ExamAPI.Services;
using Model.DTO;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace ExamifyAPI.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentModel>> GetAllStudentsAsync(int instituteId);
        Task<StudentModel?> GetStudentByIdAsync(int instituteId, int studentId);
        Task<int> InsertOrUpdateStudentAsync(StudentDTO dto, int? studentId = null, int? createdBy = null, int? modifiedBy = null);
        Task<int> AssignStudentToClassAsync(int studentId, int classId, int createdBy);
        Task<int> AssignStudentToBatchAsync(int studentId, int batchId, int createdBy);
    }

    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public StudentService(IStudentRepository studentRepository, IAuthService authService, IConfiguration config)
        {
            _studentRepository = studentRepository;
            _authService = authService;
            _config = config;
        }

        public async Task<IEnumerable<StudentModel>> GetAllStudentsAsync(int instituteId)
        {
            return await _studentRepository.GetAllStudentsAsync(instituteId);
        }

        public async Task<StudentModel?> GetStudentByIdAsync(int instituteId, int studentId)
        {
            return await _studentRepository.GetStudentByIdAsync(instituteId, studentId);
        }

        public async Task<int> InsertOrUpdateStudentAsync(StudentDTO dto, int? studentId = null, int? createdBy = null, int? modifiedBy = null)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Create user account if new student
                if (studentId == null)
                {
                    var userId = await _authService.Register(dto.UserName, dto.Password, "Student");
                    dto.UserId = userId;
                }

                // Save Student
                var newStudentId = await _studentRepository.InsertOrUpdateStudentAsync(dto, studentId, createdBy, modifiedBy);

                // Assign class if provided
                if (dto.ClassId.HasValue)
                {
                    await _studentRepository.InsertStudentClassAsync(newStudentId, dto.ClassId.Value, createdBy ?? dto.UserId);
                }

                // Assign batch if provided
                if (dto.BatchId.HasValue)
                {
                    await _studentRepository.InsertStudentBatchAsync(newStudentId, dto.BatchId.Value, createdBy ?? dto.UserId);
                }

                transaction.Commit();
                return newStudentId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> AssignStudentToClassAsync(int studentId, int classId, int createdBy)
        {
            return await _studentRepository.InsertStudentClassAsync(studentId, classId, createdBy);
        }

        public async Task<int> AssignStudentToBatchAsync(int studentId, int batchId, int createdBy)
        {
            return await _studentRepository.InsertStudentBatchAsync(studentId, batchId, createdBy);
        }
    }
}
