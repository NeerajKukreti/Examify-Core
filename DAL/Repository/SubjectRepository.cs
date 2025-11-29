using Dapper;
using DataModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model.DTO;
using System.Data;

namespace DAL.Repository
{
    public interface ISubjectRepository
    {
        Task<IEnumerable<SubjectModel>> GetAllSubjectsAsync(int instituteId, int? subjectId = null);
        Task<int> InsertOrUpdateSubjectAsync(SubjectDTO dto, int? subjectId = null, int? userId = null);
        Task InsertOrUpdateTopicsAsync(List<SubjectTopicDTO> topics, int userId);
        Task<IEnumerable<SubjectTopicModel>> GetTopicsBySubjectIdAsync(int subjectId);
        Task<bool> ChangeStatus(int subjectId);
    }

    public class SubjectRepository : ISubjectRepository
    {
        private readonly IConfiguration _config;
        public SubjectRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection() => 
            new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<SubjectModel>> GetAllSubjectsAsync(int instituteId, int? subjectId = null)
        {
            using var connection = CreateConnection();
            var parameters = new { InstituteId = instituteId, SubjectId = subjectId };
            return await connection.QueryAsync<SubjectModel>(
                "_sp_GetAllSubjects",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> InsertOrUpdateSubjectAsync(SubjectDTO dto, int? subjectId = null, int? userId = null)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SubjectId", subjectId);
            parameters.Add("@InstituteId", dto.InstituteId);
            parameters.Add("@SubjectName", dto.SubjectName);
            parameters.Add("@Description", dto.Description);
            parameters.Add("@Image", dto.Image);
            parameters.Add("@UserId", userId);

            return await connection.ExecuteScalarAsync<int>(
                "_sp_InsertUpdateSubject",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task InsertOrUpdateTopicsAsync(List<SubjectTopicDTO> topics, int userId)
        {
            using var connection = CreateConnection();
            
            var topicTable = new DataTable();
            topicTable.Columns.Add("TopicId", typeof(int));
            topicTable.Columns.Add("SubjectId", typeof(int));
            topicTable.Columns.Add("IsActive", typeof(bool));
            topicTable.Columns.Add("TopicName", typeof(string));
            topicTable.Columns.Add("Description", typeof(string));

            if (topics != null && topics.Any())
            {
                foreach (var topic in topics)
                {
                    topicTable.Rows.Add(
                        topic.TopicId > 0 ? (object)topic.TopicId : DBNull.Value,
                        topic.SubjectId,
                        topic.IsActive,
                        topic.TopicName,
                        topic.Description ?? (object)DBNull.Value
                    );
                }
            }

            var parameters = new DynamicParameters();
            parameters.Add("@Topic", topicTable.AsTableValuedParameter("dbo.SubjectTopic"));
            parameters.Add("@UserId", userId);

            await connection.ExecuteAsync(
                "_sp_InsertUpdateTopic",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<SubjectTopicModel>> GetTopicsBySubjectIdAsync(int subjectId)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<SubjectTopicModel>(
                "_sp_GetTopicsBySubjectId",
                new { SubjectId = subjectId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> ChangeStatus(int subjectId)
        {
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE Subject SET IsActive = 1 - IsActive WHERE SubjectId = @SubjectId",
                new { SubjectId = subjectId },
                commandType: CommandType.Text
            );
            return rowsAffected > 0;
        }
    }
}
