using DAL.Repository;
using DataModel;
using Model.DTO;

namespace ExamifyAPI.Services
{
    public interface IClassService
    {
        Task<IEnumerable<ClassModel>> GetAllClassesAsync(int instituteId, int? classId = null);
        Task<bool> ChangeStatus(int classId);
        Task<int> InsertOrUpdateClassAsync(
            ClassDTO dto,
            int? classId = null,
            int? userId = null
        );
        Task<IEnumerable<StudentClassModel>> GetStudentClassesAsync(int studentId);
    }

    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly IBatchRepository _batchRepository;

        public ClassService(IClassRepository classRepository, IBatchRepository batchRepository)
        {
            _classRepository = classRepository;
            _batchRepository = batchRepository;
        }

        public async Task<IEnumerable<ClassModel>> GetAllClassesAsync(int instituteId, int? classId = null)
        {
            return await _classRepository.GetAllClassesAsync(instituteId, classId);
        }

        public async Task<int> InsertOrUpdateClassAsync(ClassDTO dto, int? classId, int? userId)
        {
            // Business Rule: must have at least one batch
            if (dto.Batches == null || !dto.Batches.Any())
                throw new ArgumentException("A class must have at least one batch.");

            // Insert/Update Class
            var newClassId = await _classRepository.InsertOrUpdateClassAsync(dto, classId, userId); 

            return newClassId;
        }
        public async Task<bool> ChangeStatus(int classId)
        {
            return await _classRepository.ChangeStatus(classId);
        }

        public async Task<IEnumerable<StudentClassModel>> GetStudentClassesAsync(int studentId)
        {
            return await _classRepository.GetStudentClassesAsync(studentId);
        }
    }
}
