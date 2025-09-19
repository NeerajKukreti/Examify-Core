using DAL.Repository;
using DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExamifyAPI.Services
{
    public interface ISubjectService
    {
        Task<List<SubjectModel>> GetSubjectsAsync(int instituteId, int? subjectId = null);
        Task<List<SubjectTopicModel>> GetSubjectTopicsAsync(int instituteId, int? subjectId = null, int? topicId = null);
    }

    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;
        public SubjectService(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        public async Task<List<SubjectModel>> GetSubjectsAsync(int instituteId, int? subjectId = null)
        {
            return await _subjectRepository.GetSubjectsAsync(instituteId, subjectId);
        }

        public async Task<List<SubjectTopicModel>> GetSubjectTopicsAsync(int instituteId, int? subjectId = null, int? topicId = null)
        {
            return await _subjectRepository.GetSubjectTopicsAsync(instituteId, subjectId, topicId);
        }
    }
}
