using Dapper;
using DataModel;
using DataModel.Exam;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
                //var sessionId = 169;

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
                    .Select(g => {
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

        public int SubmitExamResponses(ExamSubmissionModel submission
        )
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
                    return 0;
                }
            }
        }

        public ExamResultModel GetExamResult(int sessionId)
        {
            using var connection = Connection;
            {
                // Get exam session details using existing table structure
                var sessionQuery = @"
                    SELECT ues.UserExamSessionId as SessionId, ues.ExamId, 
                           e.ExamName, ues.UserId, ues.SubmitTime as SubmittedAt,
                           e.MarksPerQuestion, e.NegativeMarks, ues.TotalScore
                    FROM UserExamSession ues
                    INNER JOIN Exam e ON ues.ExamId = e.ExamId
                    WHERE ues.UserExamSessionId = @SessionId";

                var session = connection.QueryFirstOrDefault(sessionQuery, new { SessionId = sessionId });
                if (session == null) return null;

                // Get user responses and calculate results
                var responseQuery = @"
                    SELECT 
                    ur.QuestionId,
                    ur.SelectedChoiceId,
                    q.QuestionEnglish AS QuestionText,
                    st.TopicName,
                    qc_selected.ChoiceTextEnglish AS SelectedChoiceText,
                    qc_correct.ChoiceId AS CorrectChoiceId,
                    qc_correct.ChoiceTextEnglish AS CorrectChoiceText,
                    CASE 
                        WHEN ur.SelectedChoiceId = qc_correct.ChoiceId THEN 1 ELSE 0 
                    END AS IsCorrect,
                    CASE 
                        WHEN ur.SelectedChoiceId IS NOT NULL THEN 1 ELSE 0 
                    END AS IsAttempted
                FROM ExamResponse ur
                INNER JOIN Question q 
                    ON ur.QuestionId = q.QuestionId
                INNER JOIN SubjectTopic st 
                    ON q.TopicId = st.TopicId
                LEFT JOIN QuestionChoice qc_selected 
                    ON ur.SelectedChoiceId = qc_selected.ChoiceId
                   AND q.QuestionId = qc_selected.QuestionId 
                INNER JOIN QuestionChoice qc_correct 
                    ON q.QuestionId = qc_correct.QuestionId 
                   AND qc_correct.IsCorrect = 1
                WHERE ur.UserExamSessionId = @sessionId
                ORDER BY q.QuestionId;";

                var questionResults = connection.Query(responseQuery, new { SessionId = sessionId }).ToList();

                // Get pairing responses
                var pairQuery = @"
                    SELECT ResponsePairId, ResponseId, LeftText, RightText
                    FROM ExamResponsePair
                    WHERE ResponseId IN (SELECT ResponseId FROM ExamResponse WHERE UserExamSessionId = @SessionId)";
                var responsePairs = connection.Query<ExamResponsePairModel>(pairQuery, new { SessionId = sessionId }).ToList();

                var orderQuery = @"
                    SELECT ResponseOrderId, ResponseId, UserOrder
                    FROM ExamResponseOrder
                    WHERE ResponseId IN (SELECT ResponseId FROM ExamResponse WHERE UserExamSessionId = @SessionId)";
                var responseOrders = connection.Query<ExamResponseOrderModel>(orderQuery, new { SessionId = sessionId }).ToList();

                // Calculate results
                var correctAnswers = questionResults.Count(q => q.IsCorrect == 1);
                var wrongAnswers = questionResults.Count(q => q.IsAttempted == 1 && q.IsCorrect == 0);
                var unattempted = questionResults.Count(q => q.IsAttempted == 0);

                var marksPerQuestion = (decimal)session.MarksPerQuestion;
                var negativeMarks = (decimal)session.NegativeMarks;

                var obtainedMarks = (correctAnswers * marksPerQuestion) - (wrongAnswers * negativeMarks);
                var totalMarks = questionResults.Count * marksPerQuestion;
                var percentage = totalMarks > 0 ? (obtainedMarks / totalMarks) * 100 : 0;

                return new ExamResultModel
                {
                    SessionId = sessionId,
                    ExamId = session.ExamId,
                    ExamName = session.ExamName,
                    UserId = session.UserId,
                    TotalMarks = totalMarks,
                    ObtainedMarks = obtainedMarks,
                    Percentage = percentage,
                    TotalQuestions = questionResults.Count,
                    CorrectAnswers = correctAnswers,
                    WrongAnswers = wrongAnswers,
                    UnattemptedQuestions = unattempted,
                    TotalTimeSpent = 0, // Will be calculated from individual responses if needed
                    SubmittedAt = session.SubmittedAt ?? DateTime.Now,
                    QuestionResults = questionResults.Select(q => new QuestionResultModel
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText,
                        TopicName = q.TopicName,
                        SelectedChoiceId = q.SelectedChoiceId,
                        SelectedChoiceText = q.SelectedChoiceText ?? "Not Attempted",
                        CorrectChoiceId = q.CorrectChoiceId,
                        CorrectChoiceText = q.CorrectChoiceText,
                        IsCorrect = q.IsCorrect == 1,
                        IsAttempted = q.IsAttempted == 1,
                        MarksAwarded = q.IsCorrect == 1 ? marksPerQuestion : (q.IsAttempted == 1 ? -negativeMarks : 0),
                        TimeSpent = 0,
                        ResponsePairs = responsePairs.Where(rp => rp.ResponseId == q.QuestionId).ToList(),
                        ResponseOrders = responseOrders.Where(ro => ro.ResponseId == q.QuestionId).ToList()
                    }).ToList(),
                    ResponsePairs = responsePairs,
                    ResponseOrders = responseOrders
                };
            }
        }
    }
}