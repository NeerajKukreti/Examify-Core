using DAL.Repository;
using DataModel;
using Model.DTO;

namespace ExamifyAPI.Services
{
    public interface IBatchService
    {
        Task<BatchModel?> GetBatchByIdAsync(int batchId);
        Task<IEnumerable<BatchModel>> GetBatchesByClassIdAsync(int classId);
        
    }

    public class BatchService : IBatchService
    {
        private readonly IBatchRepository _batchRepository;

        public BatchService(IBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task<BatchModel?> GetBatchByIdAsync(int batchId)
        {
            return await _batchRepository.GetBatchByIdAsync(batchId);
        }

        public async Task<IEnumerable<BatchModel>> GetBatchesByClassIdAsync(int classId)
        {
            return await _batchRepository.GetBatchesByClassIdAsync(classId);
        }
         

        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public T? Data { get; set; }
        }
    }
}
