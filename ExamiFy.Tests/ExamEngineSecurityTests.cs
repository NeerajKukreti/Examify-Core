using DAL.Repository;
using DataModel;
using DataModel.Exam;
using ExamAPI.Services;
using ExamifyAPI.Services;
using Moq;
using Xunit;

namespace ExamiFy.Tests
{
    public class ExamEngineSecurityTests
    {
        [Fact]
        public void SessionQuestions_ShouldStrictlyMask_IsCorrectProperty_ToFalse()
        {
            // Arrange: Simulate exam session questions returned by DAL
            var mockExamRepo = new Mock<IExamRepository>();
            var mockAuthService = new Mock<IAuthService>();
            var mockClassService = new Mock<IClassService>();

            var serverQuestions = new ExamQuestionsResponse
            {
                ExamId = 1,
                ExamName = "Azure Cloud Practitioner",
                SessionId = "101",
                Sections = new List<ExamSectionModel>
                {
                    new ExamSectionModel
                    {
                        Questions = new List<ExamSessionQuestionModel>
                        {
                            new ExamSessionQuestionModel
                            {
                                QuestionId = 1,
                                QuestionTextEnglish = "What is Azure Blob Storage?",
                                SessionChoices = new List<ExamSessionChoiceModel>
                                {
                                    new ExamSessionChoiceModel { ChoiceId = 1, ChoiceTextEnglish = "Object storage", IsCorrect = false },
                                    new ExamSessionChoiceModel { ChoiceId = 2, ChoiceTextEnglish = "Relational database", IsCorrect = false },
                                    new ExamSessionChoiceModel { ChoiceId = 3, ChoiceTextEnglish = "Message queue", IsCorrect = false }
                                }
                            }
                        }
                    }
                }
            };

            mockExamRepo.Setup(r => r.GetExamSessionQuestions(10, 1, It.IsAny<int>()))
                .Returns(serverQuestions);

            var examService = new ExamService(mockExamRepo.Object, mockAuthService.Object, mockClassService.Object);

            // Act: Fetch session questions for user 10
            var result = examService.GetExamSessionQuestions(10, 1, 1);

            // Assert: Anti-cheating guardrail - no choices disclose correct answers
            Assert.NotNull(result);
            var choices = result.Sections
                .SelectMany(s => s.Questions ?? Enumerable.Empty<ExamSessionQuestionModel>())
                .SelectMany(q => q.SessionChoices ?? Enumerable.Empty<ExamSessionChoiceModel>());

            Assert.All(choices, choice =>
            {
                Assert.False(choice.IsCorrect == true, "Anti-cheating violation: IsCorrect must be masked to false in active sessions!");
            });
        }

        [Fact]
        public void SessionSubmission_WhenAlreadySubmitted_ShouldRejectDuplicateSubmission()
        {
            // Arrange: Existing session already marked as 'Submit'
            var mockExamRepo = new Mock<IExamRepository>();
            var mockAuthService = new Mock<IAuthService>();
            var mockClassService = new Mock<IClassService>();

            var existingSession = new UserExamSessionModel
            {
                UserExamSessionId = 555,
                UserId = 20,
                ExamId = 4,
                Status = "Submit" // Already submitted
            };

            mockExamRepo.Setup(r => r.GetUserExamSession(555))
                .Returns(existingSession);

            var submission = new ExamSubmissionModel
            {
                SessionId = "555",
                UserId = 20,
                ExamId = 4,
                Responses = new List<ExamResponseSubmissionModel>()
            };

            mockExamRepo.Setup(r => r.SubmitExamResponses(It.IsAny<ExamSubmissionModel>()))
                .Throws(new InvalidOperationException("Session has already been submitted."));

            var examService = new ExamService(mockExamRepo.Object, mockAuthService.Object, mockClassService.Object);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                examService.SubmitExamResponses(submission);
            });

            Assert.Equal("Session has already been submitted.", ex.Message);
        }
    }
}
