using DAL.Repository;
using DataModel;
using ExamAPI.Services;
using ExamifyAPI.Services;
using Model.DTO;
using Moq;
using Xunit;

namespace ExamiFy.Tests
{
    public class PersonalTenantQuotaTests
    {
        [Fact]
        public async Task PersonalUser_CanTakePublicExam_WithoutInstituteClassEnrollment()
        {
            // Arrange
            var mockExamRepo = new Mock<IExamRepository>();
            var mockAuthService = new Mock<IAuthService>();
            var mockClassService = new Mock<IClassService>();

            int userId = 42;
            int examId = 100;

            mockAuthService.Setup(a => a.GetCurrentUserID()).Returns(userId);
            mockAuthService.Setup(a => a.GetCurrentInstituteId()).Returns(999); // Personal Tenant Id
            mockAuthService.Setup(a => a.GetCurrentTenantType()).Returns(1); // 1 = Personal

            // User has no previous attempts for this exam
            mockExamRepo.Setup(r => r.GetUserExamsAsync(It.IsAny<List<long>>()))
                .ReturnsAsync(new List<UserExamDTO>());

            // Public platform exam with no classes
            var publicExam = new ExamModel
            {
                ExamId = examId,
                ExamName = "Azure Fundamentals Practice Test",
                IsPublic = true,
                IsPublished = true,
                IsActive = true,
                ClassIds = new List<int>() // No institute class attached!
            };

            mockExamRepo.Setup(r => r.GetExamById(examId, 999)).Returns((ExamModel?)null);
            mockExamRepo.Setup(r => r.GetExamById(examId, 0)).Returns(publicExam);

            // User has taken 3 exams this month (well under the 10 limit)
            mockExamRepo.Setup(r => r.GetMonthlyExamCountAsync(userId)).ReturnsAsync(3);

            // User has NO institute classes
            mockClassService.Setup(c => c.GetStudentClassesAsync(userId))
                .ReturnsAsync(new List<StudentClassModel>());

            var examService = new ExamService(mockExamRepo.Object, mockAuthService.Object, mockClassService.Object);

            // Act
            var sessionExam = await examService.GetSessionExamByIdAsync(examId);

            // Assert: Personal user should be granted exam session without any class enrollment
            Assert.NotNull(sessionExam);
            Assert.Equal("Azure Fundamentals Practice Test", sessionExam.ExamName);
            Assert.True(sessionExam.IsPublic);
        }

        [Fact]
        public async Task PersonalUser_WhenExceeding10ExamsPerMonth_ShouldBeBlockedByQuota()
        {
            // Arrange
            var mockExamRepo = new Mock<IExamRepository>();
            var mockAuthService = new Mock<IAuthService>();
            var mockClassService = new Mock<IClassService>();

            int userId = 42;
            int examId = 101;

            mockAuthService.Setup(a => a.GetCurrentUserID()).Returns(userId);
            mockAuthService.Setup(a => a.GetCurrentInstituteId()).Returns(999);
            mockAuthService.Setup(a => a.GetCurrentTenantType()).Returns(1); // 1 = Personal

            mockExamRepo.Setup(r => r.GetUserExamsAsync(It.IsAny<List<long>>()))
                .ReturnsAsync(new List<UserExamDTO>());

            var publicExam = new ExamModel
            {
                ExamId = examId,
                ExamName = "AI-103 Practice Test",
                IsPublic = true,
                IsPublished = true,
                IsActive = true
            };

            mockExamRepo.Setup(r => r.GetExamById(examId, 0)).Returns(publicExam);

            // User has ALREADY taken 10 exams this month (limit reached)
            mockExamRepo.Setup(r => r.GetMonthlyExamCountAsync(userId)).ReturnsAsync(10);

            var examService = new ExamService(mockExamRepo.Object, mockAuthService.Object, mockClassService.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await examService.GetSessionExamByIdAsync(examId);
            });

            Assert.Contains("Monthly free exam limit reached", ex.Message);
        }

        [Fact]
        public async Task InstituteExam_IsNotAccessible_UnlessUserEnrolledInClass()
        {
            // Arrange
            var mockExamRepo = new Mock<IExamRepository>();
            var mockAuthService = new Mock<IAuthService>();
            var mockClassService = new Mock<IClassService>();

            int userId = 42;
            int examId = 200;

            mockAuthService.Setup(a => a.GetCurrentUserID()).Returns(userId);
            mockAuthService.Setup(a => a.GetCurrentInstituteId()).Returns(5);
            mockAuthService.Setup(a => a.GetCurrentTenantType()).Returns(2); // Institute

            mockExamRepo.Setup(r => r.GetUserExamsAsync(It.IsAny<List<long>>()))
                .ReturnsAsync(new List<UserExamDTO>());

            // Private Institute Exam restricted to Class 10
            var privateExam = new ExamModel
            {
                ExamId = examId,
                ExamName = "Physics Mock Test - Batch 2026",
                IsPublic = false, // Private to institute!
                IsPublished = true,
                IsActive = true,
                ClassIds = new List<int> { 10 }
            };

            mockExamRepo.Setup(r => r.GetExamById(examId, 5)).Returns(privateExam);

            // Student is enrolled in Class 12, NOT Class 10
            mockClassService.Setup(c => c.GetStudentClassesAsync(userId))
                .ReturnsAsync(new List<StudentClassModel> { new StudentClassModel { ClassId = 12 } });

            var examService = new ExamService(mockExamRepo.Object, mockAuthService.Object, mockClassService.Object);

            // Act
            var sessionExam = await examService.GetSessionExamByIdAsync(examId);

            // Assert: Must be null because student is not enrolled in Class 10
            Assert.Null(sessionExam);
        }
    }
}
