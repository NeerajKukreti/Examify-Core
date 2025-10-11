using DAL.Repository;
using DataModel;
using ExamAPI.Services;
using ExamifyAPI.Services;
using Model.DTO;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace ExamifyAPI.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentModel>> GetAllStudentsAsync(int instituteId, int? studentId); 
        Task<int> InsertOrUpdateStudentAsync(StudentDTO dto, int? studentId = null, int? createdBy = null, int? modifiedBy = null);
        Task<int> AssignStudentToClassAsync(int studentId, int classId, int createdBy);
        Task<int> AssignStudentToBatchAsync(int studentId, int batchId, int createdBy);
        Task<bool> ChangeStatus(int studentId);
    }

    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public StudentService(IStudentRepository studentRepository, IUserService userService, IConfiguration config)
        {
            _studentRepository = studentRepository;
            _userService = userService;
            _config = config;
        }

        public async Task<IEnumerable<StudentModel>> GetAllStudentsAsync(int instituteId, int? studentId)
        {
            return await _studentRepository.GetAllStudentsAsync(instituteId, studentId);
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
                    var userId = await _userService.CreateUserAsync(dto.UserName, dto.Password, "Student");
                    dto.UserId = userId;
                }

                // Save Student
                var newStudentId = await _studentRepository.InsertOrUpdateStudentAsync(dto, studentId, createdBy, modifiedBy);

                // Assign batch if provided
                if (dto.BatchId.HasValue)
                {
                    await _studentRepository.InsertStudentBatchAsync(newStudentId, dto.BatchId.Value, createdBy ?? dto.UserId);
                }

                transaction.Commit();
                return newStudentId;
            }
            catch (Exception e)
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

        public async Task<bool> ChangeStatus(int studentId) {
            return await _studentRepository.ChangeStatus(studentId);
        }
    }
}
