using DAL.Repository;
using DataModel;
using Model.DTO;

namespace ExamifyAPI.Services
{
    public interface ISubjectService
    {
        Task<IEnumerable<SubjectModel>> GetAllSubjectsAsync(int instituteId, int? subjectId = null);
        Task<int> InsertOrUpdateSubjectAsync(SubjectDTO dto, int? subjectId = null, int? userId = null);
        Task<IEnumerable<SubjectTopicModel>> GetTopicsBySubjectIdAsync(int subjectId);
        Task<bool> ChangeStatusAsync(int subjectId);
    }

    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;

        public SubjectService(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<IEnumerable<SubjectModel>> GetAllSubjectsAsync(int instituteId, int? subjectId = null)
        {
            return await _subjectRepository.GetAllSubjectsAsync(instituteId, subjectId);
        }

        public async Task<int> InsertOrUpdateSubjectAsync(SubjectDTO dto, int? subjectId = null, int? userId = null)
        {
            
                var newSubjectId = await _subjectRepository.InsertOrUpdateSubjectAsync(dto, subjectId, userId);


                if (dto.Topics != null && dto.Topics.Any())
                {
                    foreach (var topic in dto.Topics)
                    {
                        topic.SubjectId = newSubjectId;
                    }
                }
                await _subjectRepository.InsertOrUpdateTopicsAsync(dto.Topics, userId ?? 0);

                return newSubjectId;
            
        }


        public async Task<IEnumerable<SubjectTopicModel>> GetTopicsBySubjectIdAsync(int subjectId)
        {
            return await _subjectRepository.GetTopicsBySubjectIdAsync(subjectId);
        }

        public async Task<bool> ChangeStatusAsync(int subjectId)
        {
            return await _subjectRepository.ChangeStatus(subjectId);
        }
    }
}
