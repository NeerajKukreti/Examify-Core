using Dapper;
using DataModel;
using DataModel.Exam;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace DAL.Repository
{
    public interface IQuestionRepository
    { 
        // CRUD for Question Bank
        Task<List<QuestionModel>> GetAllQuestionsAsync();
        Task<QuestionModel?> GetQuestionByIdAsync(int id);
        Task<int> CreateQuestionAsync(QuestionModel model);
        Task<int> UpdateQuestionAsync(QuestionModel model);
        Task<int> DeleteQuestionAsync(int id);
        Task<List<QuestionTypeModel>> GetQuestionTypesAsync();
    }

    public class QuestionRepository : IQuestionRepository
    {
        private readonly IConfiguration _config;
        public QuestionRepository(IConfiguration config) => _config = config;

        private IDbConnection Connection => new SqlConnection(_config.GetConnectionString("DefaultConnection"));
 
        public async Task<List<QuestionModel>> GetAllQuestionsAsync()
        {
            using var connection = Connection;
            var result = await connection.QueryAsync<QuestionModel>("_sp_GetAllQuestions", commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<QuestionModel?> GetQuestionByIdAsync(int id)
        {
            using var connection = Connection;
            return await connection.QueryFirstOrDefaultAsync<QuestionModel>("_sp_GetAllQuestions", new { id = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateQuestionAsync(QuestionModel model)
        {
            using var connection = Connection;

            // Prepare table-valued parameter for choices (Options)
            var dtChoices = new DataTable();
            dtChoices.Columns.Add("Text1", typeof(string)); // English text
            dtChoices.Columns.Add("Text2", typeof(string)); // Hindi text (not available currently -> null)
            dtChoices.Columns.Add("Flag", typeof(bool));    // IsCorrect

            if (model.Options != null)
            {
                foreach (var opt in model.Options)
                {
                    // Assuming only one language (Text) is provided; Hindi left null
                    dtChoices.Rows.Add(opt?.Text ?? string.Empty, DBNull.Value, opt?.IsCorrect ?? false);
                }
            }

            var p = new DynamicParameters();
            p.Add("@TopicId", model.TopicId, DbType.Int32);
            p.Add("@QuestionEnglish", model.QuestionEnglish, DbType.String);
            p.Add("@QuestionHindi", string.IsNullOrWhiteSpace(model.QuestionHindi) ? null : model.QuestionHindi, DbType.String);
            p.Add("@AdditionalTextEnglish", string.IsNullOrWhiteSpace(model.AdditionalTextEnglish) ? null : model.AdditionalTextEnglish, DbType.String);
            p.Add("@AdditionalTextHindi", string.IsNullOrWhiteSpace(model.AdditionalTextHindi) ? null : model.AdditionalTextHindi, DbType.String);
            p.Add("@Explanation", string.IsNullOrWhiteSpace(model.Explanation) ? null : model.Explanation, DbType.String);
            p.Add("@QuestionTypeId", model.QuestionTypeId, DbType.Int32);
            // Stored procedure expects NVARCHAR(100) CreatedBy, model has int? so convert
            p.Add("@CreatedBy", model.CreatedBy, DbType.Int32);
            p.Add("@IsMultiSelect", model.IsMultiSelect, DbType.Boolean);
            p.Add("@Choices", dtChoices.AsTableValuedParameter("dbo.CommonTextFlagType"));

            return await connection.ExecuteScalarAsync<int>("_sp_CreateQuestion", p, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> UpdateQuestionAsync(QuestionModel model)
        {
            using var connection = Connection;
            return await connection.ExecuteScalarAsync<int>("_sp_UpdateQuestion", new {
                model.QuestionId,
                model.QuestionEnglish,
                model.QuestionHindi,
                model.TopicId,
                //model.TopicName,
                model.IsMultiSelect
            }, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> DeleteQuestionAsync(int id)
        {
            using var connection = Connection;
            return await connection.ExecuteScalarAsync<int>("_sp_DeleteQuestion", new { QuestionId = id }, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<QuestionTypeModel>> GetQuestionTypesAsync()
        {
            using var connection = Connection;
            var result = await connection.QueryAsync<QuestionTypeModel>("_sp_GetQuesionType", commandType: CommandType.StoredProcedure);
            return result.ToList();
        }
    }
}