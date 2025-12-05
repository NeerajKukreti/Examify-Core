using Dapper;
using DataModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace DAL.Repository
{
    public interface IQuestionExtractorRepository
    {
        Task<int> SaveExtractedQuestionAsync(QuestionModel model, int instituteId);
        Task<List<int>> SaveExtractedQuestionsAsync(List<QuestionModel> questions, int instituteId);
    }

    public class QuestionExtractorRepository : IQuestionExtractorRepository
    {
        private readonly IConfiguration _config;
        public QuestionExtractorRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<int> SaveExtractedQuestionAsync(QuestionModel model, int instituteId)
        {
            using var connection = CreateConnection();

            var dtChoices = new DataTable();
            dtChoices.Columns.Add("Id", typeof(string));
            dtChoices.Columns.Add("Text1", typeof(string));
            dtChoices.Columns.Add("Text2", typeof(string));
            dtChoices.Columns.Add("Order", typeof(string));
            dtChoices.Columns.Add("Flag", typeof(bool));

            if (model.Options != null)
            {
                foreach (var opt in model.Options)
                {
                    dtChoices.Rows.Add(opt?.ChoiceId, opt?.Text ?? string.Empty, DBNull.Value, DBNull.Value, opt?.IsCorrect ?? false);
                }
            }

            var classIdsTable = new DataTable();
            classIdsTable.Columns.Add("id", typeof(int));
            if (model.ClassIds != null)
            {
                foreach (var cid in model.ClassIds)
                {
                    classIdsTable.Rows.Add(cid);
                }
            }

            var p = new DynamicParameters();
            p.Add("@classIds", classIdsTable.AsTableValuedParameter("IntList"));
            p.Add("@QuestionId", model.QuestionId, DbType.Int32);
            p.Add("@TopicId", model.TopicId, DbType.Int32);
            p.Add("@QuestionEnglish", model.QuestionEnglish, DbType.String);
            p.Add("@QuestionHindi", string.IsNullOrWhiteSpace(model.QuestionHindi) ? null : model.QuestionHindi, DbType.String);
            p.Add("@AdditionalTextEnglish", string.IsNullOrWhiteSpace(model.AdditionalTextEnglish) ? null : model.AdditionalTextEnglish, DbType.String);
            p.Add("@AdditionalTextHindi", string.IsNullOrWhiteSpace(model.AdditionalTextHindi) ? null : model.AdditionalTextHindi, DbType.String);
            p.Add("@Explanation", string.IsNullOrWhiteSpace(model.Explanation) ? null : model.Explanation, DbType.String);
            p.Add("@DifficultyLevel", model.DifficultyLevel, DbType.String);
            p.Add("@QuestionTypeId", model.QuestionTypeId, DbType.Int32);
            p.Add("@UserId", model.CreatedBy, DbType.Int32);
            p.Add("@IsMultiSelect", model.IsMultiSelect, DbType.Boolean);
            p.Add("@Choices", dtChoices.AsTableValuedParameter("dbo.CommonTextFlagType"));
            p.Add("@InstituteId", instituteId, DbType.Int32);

            return await connection.ExecuteScalarAsync<int>("_sp_UpsertQuestion", p, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<int>> SaveExtractedQuestionsAsync(List<QuestionModel> questions, int instituteId)
        {
            var questionIds = new List<int>();
            foreach (var question in questions)
            {
                var id = await SaveExtractedQuestionAsync(question, instituteId);
                questionIds.Add(id);
            }
            return questionIds;
        }
    }
}
