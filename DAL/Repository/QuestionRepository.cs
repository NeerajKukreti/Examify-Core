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
        Task<QuestionModel?> GetQuestionAsync(int id);
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

        public async Task<QuestionModel?> GetQuestionAsync(int id)
        {
            using var connection = Connection;
            var p = new { QuestionId = id };
            using var multi = await connection.QueryMultipleAsync(
                "_sp_GetAllQuestions", p, commandType: CommandType.StoredProcedure);

            var question = await multi.ReadFirstOrDefaultAsync<QuestionModel>();

            if (question == null)
                return null;

            // Second result: Options OR Pairs depending on type
            if (question.TypeName is "MCQ" or "True/False" or "Descriptive")
            {
                var optionRows = await multi.ReadAsync();
                question.Options = optionRows.Select(opt => new OptionModel
                {
                    ChoiceId = opt.ChoiceId,
                    Text = (string)opt.ChoiceTextEnglish,
                    IsCorrect = (bool)opt.IsCorrect
                }).ToList();
            }
            else if (question.TypeName == "Matching")
            {
                var pairRows = await multi.ReadAsync();
                question.Pairs = pairRows.Select(p => new PairModel
                {
                    PairId = p.PairId,
                    LeftText = (string)p.LeftText,
                    RightText = (string)p.RightText
                }).ToList();
            }
            else if (question.TypeName == "Ordering")
            {
                var orderRows = await multi.ReadAsync();
                question.Orders = orderRows.Select(p => new OrderModel
                {
                    OrderId = p.OrderId,
                    ItemText = (string)p.ItemText,
                    CorrectOrder = (int)p.CorrectOrder
                }).ToList(); 
            }

            return question;
        }

        public async Task<int> CreateQuestionAsync(QuestionModel model)
        {
            try
            {
                using var connection = Connection;

                // Prepare table-valued parameter for choices (Options)
                var dtChoices = new DataTable();
                dtChoices.Columns.Add("Id", typeof(string)); // ChioiceId
                dtChoices.Columns.Add("Text1", typeof(string)); // English text
                dtChoices.Columns.Add("Text2", typeof(string)); // Hindi text (not available currently -> null)
                dtChoices.Columns.Add("Order", typeof(string)); // ChioiceId
                dtChoices.Columns.Add("Flag", typeof(bool));    // IsCorrect

                var questtionTypes = await GetQuestionTypesAsync();
                var questionType = questtionTypes.FirstOrDefault(qt => qt.QuestionTypeId == model.QuestionTypeId);

                var choiceTypes = new[] { "MCQ", "True/False", "Descriptive" };


                if (choiceTypes.Contains(questionType?.TypeName))
                {
                    if (model.Options != null)
                    {
                        foreach (var opt in model.Options)
                        {
                            // Assuming only one language (Text) is provided; Hindi left null
                            dtChoices.Rows.Add(opt?.ChoiceId, opt?.Text ?? string.Empty, DBNull.Value, DBNull.Value, opt?.IsCorrect ?? false);
                        }
                    }
                } 
                else if (questionType?.TypeName == "Matching")
                {
                    if (model.Pairs != null)
                    {
                        foreach (var opt in model.Pairs)
                        {
                            dtChoices.Rows.Add(opt?.PairId, opt?.LeftText ?? string.Empty, opt?.RightText ?? string.Empty, DBNull.Value, DBNull.Value);
                        }
                    }
                }
                else if (questionType?.TypeName == "Ordering")
                {
                    if (model.Orders != null)
                    {
                        var i = 0;

                        foreach (var opt in model.Orders)
                        {
                            dtChoices.Rows.Add(opt?.OrderId, opt?.ItemText ?? string.Empty, DBNull.Value,++i, DBNull.Value);
                        }
                    }
                }

                var p = new DynamicParameters();
                p.Add("@QuestionId", model.QuestionId, DbType.Int32);
                p.Add("@TopicId", model.TopicId, DbType.Int32);
                p.Add("@QuestionEnglish", model.QuestionEnglish, DbType.String);
                p.Add("@QuestionHindi", string.IsNullOrWhiteSpace(model.QuestionHindi) ? null : model.QuestionHindi, DbType.String);
                p.Add("@AdditionalTextEnglish", string.IsNullOrWhiteSpace(model.AdditionalTextEnglish) ? null : model.AdditionalTextEnglish, DbType.String);
                p.Add("@AdditionalTextHindi", string.IsNullOrWhiteSpace(model.AdditionalTextHindi) ? null : model.AdditionalTextHindi, DbType.String);
                p.Add("@Explanation", string.IsNullOrWhiteSpace(model.Explanation) ? null : model.Explanation, DbType.String);
                p.Add("@DifficultyLevel", model.DifficultyLevel, DbType.String);
                p.Add("@QuestionTypeId", model.QuestionTypeId, DbType.Int32);
                // Stored procedure expects NVARCHAR(100) CreatedBy, model has int? so convert
                p.Add("@UserId", model.CreatedBy, DbType.Int32);
                p.Add("@IsMultiSelect", model.IsMultiSelect, DbType.Boolean);
                p.Add("@Choices", dtChoices.AsTableValuedParameter("dbo.CommonTextFlagType"));

                return await connection.ExecuteScalarAsync<int>("_sp_UpsertQuestion", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                throw new Exception("Error creating question", ex);
            }
        }

        public async Task<int> UpdateQuestionAsync(QuestionModel model)
        {
            using var connection = Connection;
            return await connection.ExecuteScalarAsync<int>("_sp_UpdateQuestion", new
            {
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