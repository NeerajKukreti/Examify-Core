using Dapper;
using DataModel;
using DataModel.Exam;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model.DTO;
using System.ComponentModel;
using System.Data;

namespace DAL.Repository
{
    public interface IExamRepository
    {
        List<ExamModel> GetActiveExams();
        ExamModel GetExamById(int examId);
        ExamQuestionsResponse GetExamSessionQuestions(int userId, int examId);
        int SubmitExamResponses(ExamSubmissionModel submission);
        ExamResultModel GetExamResult(int sessionId);
        Task<int> InsertOrUpdateExamAsync(ExamDTO dto, int? examId = null,
            int? userloggedIn = null);
        Task<bool> ChangeStatus(int examId);
        Task<bool> PublishExam(int examId);
        Task<IEnumerable<AvailableQuestionDTO>> GetAvailableQuestionsAsync(int examId, int instituteId);
        Task<IEnumerable<ExamQuestionDTO>> GetExamQuestionsAsync(int examId);
        Task<bool> SaveExamQuestionsAsync(ExamQuestionConfigDTO config);
        Task<bool> RemoveExamQuestionAsync(int examId, int questionId);
        Task<IEnumerable<UserExamDTO>> GetUserExamsAsync(List<long> userIds);
        Task<StatsDTO> GetStatsAsync();
    }

    public class ExamRepository : IExamRepository
    {
        private readonly IConfiguration _config;
        public ExamRepository(IConfiguration config) => _config = config;

        private IDbConnection Connection => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public List<ExamModel> GetActiveExams()
        {
            using var connection = Connection;
            {
                var exams = connection.Query<ExamModel>(
                    "_sp_GetAllExams",
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return exams;
            }
        }

        public async Task<int> InsertOrUpdateExamAsync(ExamDTO dto, int? examId = null,
            int? userloggedIn = null)
        {
            using var connection = Connection;
            var parameters = new DynamicParameters();
            parameters.Add("@ExamId", examId);
            parameters.Add("@ExamName", dto.ExamName);
            parameters.Add("@Description", dto.Description);
            parameters.Add("@Image", dto.Image);
            parameters.Add("@DurationMinutes", dto.DurationMinutes);
            parameters.Add("@TotalQuestions", dto.TotalQuestions);
            parameters.Add("@Instructions", dto.Instructions);
            parameters.Add("@ExamType", dto.ExamType);
            parameters.Add("@CutOffPercentage", dto.CutOffPercentage);
            parameters.Add("@UserId", userloggedIn);

            return await connection.ExecuteScalarAsync<int>(
                "_sp_InsertUpdateExam",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> ChangeStatus(int examId)
        {
            using var connection = Connection;
            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE Exam SET IsActive = ~IsActive WHERE ExamId = @ExamId",
                new { ExamId = examId },
                commandType: CommandType.Text
            );
            return rowsAffected > 0;
        }

        public async Task<bool> PublishExam(int examId)
        {
            using var connection = Connection;
            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE Exam SET IsPublished = ~IsPublished WHERE ExamId = @ExamId",
                new { ExamId = examId },
                commandType: CommandType.Text
            );
            return rowsAffected > 0;
        }


        #region Exam Session
        public ExamModel GetExamById(int examId)
        {
            using var connection = Connection;
            {
                var exam = connection.QueryFirstOrDefault<ExamModel>(
                    "_sp_GetAllExams",
                    new { ExamId = examId },
                    commandType: CommandType.StoredProcedure
                );

                return exam;
            }
        }

        public ExamQuestionsResponse GetExamSessionQuestions(int userId, int examId)
        {
            using var connection = Connection;
            {
                //Begin exam session first
                var sessionId = connection.QuerySingle<long>(
                    "_sp_BeginExam",
                    new { ExamId = examId, UserId = userId },
                    commandType: CommandType.StoredProcedure
                );


                // Get exam details
                var exam = connection.QueryFirstOrDefault<ExamModel>(
                    "_sp_GetAllExams",
                    new { ExamId = examId },
                    commandType: CommandType.StoredProcedure
                );

                // Get session questions with all options in one go
                var questionRows = connection.Query(
                    "_sp_GetExamSessionQuestions",
                    new { UserExamSessionId = sessionId },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                // Group by SessionQuestionId
                var sessionQuestions = questionRows
                    .Where(r => r.SessionQuestionId != null)
                    .GroupBy(r => Convert.ToInt64(r.SessionQuestionId))
                    .Select(g =>
                    {
                        var first = g.First();
                        var question = new ExamSessionQuestionModel
                        {
                            SessionQuestionId = first.SessionQuestionId ?? 0,
                            UserExamSessionId = first.UserExamSessionId ?? 0,
                            QuestionId = first.QuestionId,
                            TopicName = first.TopicName,
                            SubjectName = first.SubjectName,
                            QuestionTextEnglish = first.QuestionTextEnglish,
                            QuestionTextHindi = first.QuestionTextHindi,
                            AdditionalTextEnglish = first.AdditionalTextEnglish,
                            AdditionalTextHindi = first.AdditionalTextHindi,
                            QuestionTypeId = first.QuestionTypeId,
                            QuestionTypeName = first.QuestionTypeName,
                            IsMultiSelect = first.IsMultiSelect,
                            IsObjective = first.IsObjective,
                            Marks = first.Marks,
                            NegativeMarks = first.NegativeMarks,
                            SortOrder = first.SortOrder,
                            SessionChoices = new List<ExamSessionChoiceModel>(),
                            SessionOrders = new List<ExamSessionQuestionOrderModel>(),
                            SessionPairs = new List<ExamSessionQuestionPairModel>(),
                        };

                        // Only fill relevant collections based on QuestionTypeId
                        if (question.QuestionTypeName == "MCQ" || question.QuestionTypeName == "True/False") // MCQ/TF
                        {
                            foreach (var row in g)
                            {
                                if (row.SessionChoiceId != null)
                                {
                                    question.SessionChoices.Add(new ExamSessionChoiceModel
                                    {
                                        SessionChoiceId = row.SessionChoiceId ?? 0,
                                        SessionQuestionId = row.SessionQuestionId ?? 0,
                                        ChoiceId = row.ChoiceId ?? 0,
                                        ChoiceTextEnglish = row.ChoiceTextEnglish,
                                        ChoiceTextHindi = row.ChoiceTextHindi,
                                        IsCorrect = row.IsCorrect
                                    });
                                }
                            }
                        }
                        else if (question.QuestionTypeName == "Ordering") // Ordering
                        {
                            foreach (var row in g)
                            {
                                if (row.SessionOrderId != null && row.ItemText != null)
                                {
                                    question.SessionOrders.Add(new ExamSessionQuestionOrderModel
                                    {
                                        SessionOrderId = row.SessionOrderId ?? 0,
                                        SessionQuestionId = row.SessionQuestionId ?? 0,
                                        ItemText = row.ItemText,
                                        CorrectOrder = row.CorrectOrder ?? 0
                                    });
                                }
                            }
                        }
                        else if (question.QuestionTypeName == "Matching") // Matching
                        {
                            foreach (var row in g)
                            {
                                if (row.SessionPairId != null && row.LeftText != null && row.RightText != null)
                                {
                                    question.SessionPairs.Add(new ExamSessionQuestionPairModel
                                    {
                                        SessionPairId = row.SessionOrderId ?? 0, // Use SessionOrderId as PairId if not separate
                                        SessionQuestionId = row.SessionQuestionId ?? 0,
                                        LeftText = row.LeftText,
                                        RightText = row.RightText
                                    });
                                }
                            }
                        }
                        return question;
                    })
                    .ToList();

                // Group by TopicName for sections
                var sections = sessionQuestions
                    .GroupBy(q => q.TopicName)
                    .Select(g => new ExamSectionModel
                    {
                        SectionName = g.Key,
                        Questions = g.OrderBy(q => q.SortOrder ?? 0).ToList()
                    })
                    .OrderBy(s => s.SectionName)
                    .ToList();

                return new ExamQuestionsResponse
                {
                    ExamId = exam.ExamId,
                    ExamName = exam.ExamName,
                    DurationMinutes = exam.DurationMinutes,
                    Sections = sections,
                    SessionId = sessionId.ToString()
                };
            }
        }

        public int SubmitExamResponses(ExamSubmissionModel submission)
        {
            using var connection = Connection;
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Build DataTable for TVP
                    var responseTable = new DataTable();
                    responseTable.Columns.Add("SessionQuestionId", typeof(long));
                    responseTable.Columns.Add("SessionChoiceIds", typeof(string));
                    responseTable.Columns.Add("ResponseText", typeof(string));
                    responseTable.Columns.Add("TimeSpent", typeof(int));
                    responseTable.Columns.Add("OrderedItems", typeof(string)); // Comma-separated order indices
                    responseTable.Columns.Add("PairedItems", typeof(string)); // Comma-separated pairs

                    foreach (var response in submission.Responses)
                    {
                        string orderedItemsStr = response.OrderedItems != null && response.OrderedItems.Count > 0
                            ? string.Join(",", response.OrderedItems.Select(o => o.UserOrder))
                            : null;
                        string pairedItemsStr = response.PairedItems != null && response.PairedItems.Count > 0
                            ? string.Join("|", response.PairedItems.Select(p => $"{p.LeftText}:{p.RightText}"))
                            : null;
                        string choiceIdsStr = response.SessionChoiceIds != null && response.SessionChoiceIds.Count > 0
                            ? string.Join(",", response.SessionChoiceIds)
                            : null;
                        responseTable.Rows.Add(
                            response.SessionQuestionId,
                            choiceIdsStr,
                          !string.IsNullOrEmpty(orderedItemsStr) ? orderedItemsStr :
                          response.ResponseText ?? "",
                            response.TimeSpent,
                            orderedItemsStr,
                            pairedItemsStr
                        );
                    }

                    // Call the new bulk insert SP
                    connection.Execute(
                        "_sp_BulkInsertExamResponses",
                        new
                        {
                            UserExamSessionId = submission.SessionId,
                            Responses = responseTable.AsTableValuedParameter("dbo.ExamResponseSubmissionList")
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure
                    );

                    var sessionId = connection.QuerySingle<int>(
                        "_sp_SubmitExamSession",
                        new
                        {
                            UserExamSessionId = submission.SessionId,
                            SubmitTime = submission.SubmittedAt
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure
                    );

                    transaction.Commit();
                    return sessionId;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public ExamResultModel GetExamResult(int sessionId)
        {
            using var connection = Connection;
            using var multi = connection.QueryMultiple(
                "_sp_GetExamResults",
                new { UserExamSessionId = sessionId },
                commandType: CommandType.StoredProcedure);

            var session = multi.Read<dynamic>().FirstOrDefault();
            if (session == null) return null;

            var exam = multi.Read<dynamic>().FirstOrDefault();
            var examQuestions = multi.Read<dynamic>().ToList();
            var sessionQuestions = multi.Read<dynamic>().ToList();
            var sessionChoices = multi.Read<dynamic>().ToList();
            var sessionOrders = multi.Read<dynamic>().ToList();
            var sessionPairs = multi.Read<dynamic>().ToList();
            var responses = multi.Read<dynamic>().ToList();

            var correctAnswers = 0;
            var wrongAnswers = 0;
            var unattempted = 0;

            var questionResults = sessionQuestions.Select(q =>
            {
                var response = responses.FirstOrDefault(r => r.SessionQuestionId == q.SessionQuestionId);
                var isAttempted = response?.IsCorrect != null;
                var isCorrect = response?.IsCorrect == true;

                if (!isAttempted) unattempted++;
                else if (isCorrect) correctAnswers++;
                else wrongAnswers++;

                var choices = sessionChoices
                    .Where(c => c.SessionQuestionId == q.SessionQuestionId)
                    .Select(c => new
                    {
                        SessionChoiceId = c.SessionChoiceId,
                        ChoiceText = c.ChoiceTextEnglish,
                        IsCorrect = c.IsCorrect == true
                    }).ToList();

                var pairs = sessionPairs
                    .Where(p => p.SessionQuestionId == q.SessionQuestionId)
                    .Select(p => new ExamResponsePairModel
                    {
                        SessionPairId = p.SessionPairId ?? 0,
                        SessionQuestionId = p.SessionQuestionId ?? 0,
                        LeftText = p.LeftText,
                        RightText = p.RightText
                    }).ToList();



                return new QuestionResultModel
                {
                    QuestionId = q.SessionQuestionId,
                    QuestionText = q.QuestionTextEnglish,
                    QuestionType = q.TypeName,
                    Marks = q.Marks ?? 0,
                    NegativeMarks = q.NegativeMarks ?? 0,
                    IsMultiSelect = q.IsMultiSelect == true,
                    ResponseText = response?.ResponseText,
                    SessionChoiceId = response?.SessionChoiceId,
                    IsCorrect = isCorrect,
                    IsAttempted = isAttempted,
                    MarksAwarded = isCorrect ? (q.Marks ?? 0) : (isAttempted ? -(q.NegativeMarks ?? 0) : 0),
                    AllChoices = choices,
                    ResponsePairs = pairs,
                    ResponseOrders = sessionOrders
                        .Where(o => o.SessionQuestionId == q.SessionQuestionId)
                        .Select((o, idx) => {
                            var userOrder = 0;
                            if (!string.IsNullOrEmpty(response?.ResponseText))
                            {
                                var orderParts = response.ResponseText.Split(',');
                                if (idx < orderParts.Length)
                                {
                                    int.TryParse(orderParts[idx], out userOrder);
                                }
                            }
                            return new ExamResponseOrderModel
                            {
                                ResponseOrderId = o.SessionOrderId ?? 0,
                                SessionQuestionId = o.SessionQuestionId ?? 0,
                                ItemText = o.ItemText,
                                UserOrder = userOrder,
                                CorrectOrder = o.CorrectOrder ?? 0
                            };
                        }).ToList(),
                    CorrectChoices = choices.Where(c => c.IsCorrect).ToList()
                };
            }).ToList();

            var totalMarks = questionResults.Sum(q => q.Marks);
            var obtainedMarks = questionResults.Sum(q => q.MarksAwarded);
            var percentage = totalMarks > 0 ? (obtainedMarks / totalMarks) * 100 : 0;

            return new ExamResultModel
            {
                SessionId = sessionId,
                ExamName = exam?.ExamName ?? "Exam",
                TotalMarks = totalMarks,
                ObtainedMarks = obtainedMarks,
                Percentage = percentage,
                TotalQuestions = questionResults.Count,
                CorrectAnswers = correctAnswers,
                WrongAnswers = wrongAnswers,
                UnattemptedQuestions = unattempted,
                SubmittedAt = session.SubmitTime ?? DateTime.Now,
                QuestionResults = questionResults,
                CutOffPercentage = exam?.CutOffPercentage
            };
        }
        #endregion

        #region Exam Question Configuration
        public async Task<IEnumerable<AvailableQuestionDTO>> GetAvailableQuestionsAsync(int examId, int instituteId)
        {
            using var connection = Connection;
            var sql = @"
                SELECT q.QuestionId, q.QuestionEnglish, q.QuestionHindi, 
                       t.TopicName, s.SubjectName, qt.TypeName as QuestionTypeName,q.difficultyLevel, q.IsMultiSelect
                FROM Question q
                LEFT JOIN SubjectTopic t ON q.TopicId = t.TopicId
                LEFT JOIN Subject s ON t.SubjectId = s.SubjectId and s.InstituteId = @InstituteId 
                LEFT JOIN QuestionType qt ON q.QuestionTypeId = qt.QuestionTypeId
                WHERE q.IsDeleted = 0
                  AND q.QuestionId NOT IN (SELECT QuestionId FROM ExamQuestion WHERE ExamId = @ExamId)
                ORDER BY s.SubjectName, t.TopicName";

            return await connection.QueryAsync<AvailableQuestionDTO>(sql, new { ExamId = examId, InstituteId = instituteId });
        }

        public async Task<IEnumerable<ExamQuestionDTO>> GetExamQuestionsAsync(int examId)
        {
            using var connection = Connection;
            var sql = @"
                SELECT eq.ExamId, eq.QuestionId, eq.Marks, eq.NegativeMarks, eq.SortOrder,
                       q.QuestionEnglish, t.TopicName, qt.TypeName as QuestionTypeName
                FROM ExamQuestion eq
                INNER JOIN Question q ON eq.QuestionId = q.QuestionId
                INNER JOIN SubjectTopic t ON q.TopicId = t.TopicId
                INNER JOIN QuestionType qt ON q.QuestionTypeId = qt.QuestionTypeId
                WHERE eq.ExamId = @ExamId
                ORDER BY eq.SortOrder";

            return await connection.QueryAsync<ExamQuestionDTO>(sql, new { ExamId = examId });
        }

        public async Task<bool> SaveExamQuestionsAsync(ExamQuestionConfigDTO config)
        {
            using var connection = Connection;
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Delete existing questions
                await connection.ExecuteAsync(
                    "DELETE FROM ExamQuestion WHERE ExamId = @ExamId",
                    new { config.ExamId },
                    transaction);

                // Insert new questions
                foreach (var question in config.Questions)
                {
                    await connection.ExecuteAsync(
                        @"INSERT INTO ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder)
                          VALUES (@ExamId, @QuestionId, @Marks, @NegativeMarks, @SortOrder)",
                        question,
                        transaction);
                }

                // Update TotalQuestions in Exam table
                await connection.ExecuteAsync(
                    "UPDATE Exam SET TotalQuestions = @Count WHERE ExamId = @ExamId",
                    new { Count = config.Questions.Count, config.ExamId },
                    transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public async Task<bool> RemoveExamQuestionAsync(int examId, int questionId)
        {
            using var connection = Connection;
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM ExamQuestion WHERE ExamId = @ExamId AND QuestionId = @QuestionId",
                new { ExamId = examId, QuestionId = questionId });

            if (rowsAffected > 0)
            {
                // Update TotalQuestions count
                await connection.ExecuteAsync(
                    @"UPDATE Exam SET TotalQuestions = (SELECT COUNT(*) FROM ExamQuestion WHERE ExamId = @ExamId)
                      WHERE ExamId = @ExamId",
                    new { ExamId = examId });
            }

            return rowsAffected > 0;
        }
        #endregion

        public async Task<IEnumerable<UserExamDTO>> GetUserExamsAsync(List<long> userIds)
        {
            using var connection = Connection;
            var userIdTable = new DataTable();
            userIdTable.Columns.Add("id", typeof(int));
            foreach (var id in userIds)
                userIdTable.Rows.Add(id);

            return await connection.QueryAsync<UserExamDTO>(
                "_sp_GetUserExams",
                new { @Userids = userIdTable.AsTableValuedParameter("IntList") },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<StatsDTO> GetStatsAsync()
        {
            using var connection = Connection;
            using var multi = await connection.QueryMultipleAsync(
                "_sp_GetStats",
                commandType: CommandType.StoredProcedure);

            var stats = new StatsDTO
            {
                TotalQuestions = (await multi.ReadFirstOrDefaultAsync<dynamic>())?.TotalQuestions ?? 0,
                QuestionsByType = (await multi.ReadAsync<QuestionTypeStatsDTO>()).ToList(),
                QuestionsByDifficulty = (await multi.ReadAsync<DifficultyStatsDTO>()).ToList(),
                SubjectStats = (await multi.ReadAsync<SubjectStatsDTO>()).ToList(),
                SubjectQuestionStats = (await multi.ReadAsync<SubjectQuestionStatsDTO>()).ToList(),
                TotalClasses = (await multi.ReadFirstOrDefaultAsync<dynamic>())?.TotalClasses ?? 0
            };

            return stats;
        }
    }
}