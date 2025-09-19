using Dapper;
using DataModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace DAL.Repository
{
    public interface ISubjectRepository
    {
        Task<List<SubjectModel>> GetSubjectsAsync(int instituteId, int? subjectId = null);
        Task<List<SubjectTopicModel>> GetSubjectTopicsAsync(int instituteId, int? subjectId = null, int? topicId = null);
    }

    public class SubjectRepository : ISubjectRepository
    {
        private readonly IConfiguration _config;
        public SubjectRepository(IConfiguration config) => _config = config;
        private IDbConnection Connection => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<List<SubjectModel>> GetSubjectsAsync(int instituteId, int? subjectId = null)
        {
            using var connection = Connection;
            var result = await connection.QueryAsync<SubjectModel>("_sp_GetSubject", new { InstituteId = instituteId, SubjectId = subjectId }, commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<List<SubjectTopicModel>> GetSubjectTopicsAsync(int instituteId, int? subjectId = null, int? topicId = null)
        {
            using var connection = Connection;
            var result = await connection.QueryAsync<SubjectTopicModel>("_sp_getSubjectTopic", new { InstituteId = instituteId, SubjectId = subjectId, TopicId = topicId }, commandType: CommandType.StoredProcedure);
            return result.ToList();
        }
    }
}
