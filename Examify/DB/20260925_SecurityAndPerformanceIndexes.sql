-- ==============================================================================
-- Migration: 20260925_SecurityAndPerformanceIndexes.sql
-- Description: Creates missing non-clustered indexes on Foreign Keys & high-traffic
--              lookup columns, and hardens exam stored procedures against data leakage
--              and duplicate submissions.
-- ==============================================================================

USE [db_exam];
GO

-- 1. Batch & Class Indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Batch_ClassId' AND object_id = OBJECT_ID('dbo.Batch'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Batch_ClassId ON dbo.Batch(ClassId) INCLUDE (BatchName, IsActive);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Class_InstituteId' AND object_id = OBJECT_ID('dbo.Class'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Class_InstituteId ON dbo.Class(InstituteId) INCLUDE (ClassName, IsActive);
END
GO

-- 2. Subject & Topic Indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Subject_InstituteId' AND object_id = OBJECT_ID('dbo.Subject'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Subject_InstituteId ON dbo.Subject(InstituteId) INCLUDE (SubjectName, IsActive);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SubjectTopic_SubjectId' AND object_id = OBJECT_ID('dbo.SubjectTopic'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SubjectTopic_SubjectId ON dbo.SubjectTopic(SubjectId) INCLUDE (TopicName, IsActive);
END
GO

-- 3. Student & User Indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Student_InstituteId' AND object_id = OBJECT_ID('dbo.Student'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Student_InstituteId ON dbo.Student(InstituteId) INCLUDE (StudentName, UserId, IsActive);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Student_UserId' AND object_id = OBJECT_ID('dbo.Student'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Student_UserId ON dbo.Student(UserId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Institute_UserId' AND object_id = OBJECT_ID('dbo.Institute'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Institute_UserId ON dbo.Institute(UserId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentBatch_Composite' AND object_id = OBJECT_ID('dbo.StudentBatch'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_StudentBatch_Composite ON dbo.StudentBatch(StudentId, BatchId);
END
GO

IF OBJECT_ID('dbo.StudentClass') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentClass_Composite' AND object_id = OBJECT_ID('dbo.StudentClass'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_StudentClass_Composite ON dbo.StudentClass(StudentId, ClassId);
END
GO

-- 4. Question & Exam Question Indexes
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamQuestion_Composite' AND object_id = OBJECT_ID('dbo.ExamQuestion'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamQuestion_Composite ON dbo.ExamQuestion(ExamId, QuestionId) INCLUDE (Marks, NegativeMarks, SortOrder);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Question_TopicId' AND object_id = OBJECT_ID('dbo.Question'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Question_TopicId ON dbo.Question(TopicId) INCLUDE (QuestionTypeId, IsMultiSelect, IsActive);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QuestionChoice_QuestionId' AND object_id = OBJECT_ID('dbo.QuestionChoice'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_QuestionChoice_QuestionId ON dbo.QuestionChoice(QuestionId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QuestionOrder_QuestionId' AND object_id = OBJECT_ID('dbo.QuestionOrder'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_QuestionOrder_QuestionId ON dbo.QuestionOrder(QuestionId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_QuestionPair_QuestionId' AND object_id = OBJECT_ID('dbo.QuestionPair'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_QuestionPair_QuestionId ON dbo.QuestionPair(QuestionId);
END
GO

-- 5. Session & Response Indexes (High-throughput during exam runs)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserExamSession_ExamUser' AND object_id = OBJECT_ID('dbo.UserExamSession'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UserExamSession_ExamUser ON dbo.UserExamSession(ExamId, UserId, Status);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamSessionQuestion_Session' AND object_id = OBJECT_ID('dbo.ExamSessionQuestion'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamSessionQuestion_Session ON dbo.ExamSessionQuestion(UserExamSessionId, QuestionId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamSessionChoice_SessionQ' AND object_id = OBJECT_ID('dbo.ExamSessionChoice'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamSessionChoice_SessionQ ON dbo.ExamSessionChoice(SessionQuestionId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamSessionQuestionOrder_SessionQ' AND object_id = OBJECT_ID('dbo.ExamSessionQuestionOrder'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamSessionQuestionOrder_SessionQ ON dbo.ExamSessionQuestionOrder(SessionQuestionId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamSessionQuestionPair_SessionQ' AND object_id = OBJECT_ID('dbo.ExamSessionQuestionPair'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamSessionQuestionPair_SessionQ ON dbo.ExamSessionQuestionPair(SessionQuestionId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExamResponse_Session' AND object_id = OBJECT_ID('dbo.ExamResponse'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExamResponse_Session ON dbo.ExamResponse(UserExamSessionId, SessionQuestionId);
END
GO

-- ==============================================================================
-- 6. Hardened Stored Procedure: _sp_GetExamSessionQuestions
-- Never leak IsCorrect to active exam sessions at the database layer
-- ==============================================================================
CREATE OR ALTER PROCEDURE [dbo].[_sp_GetExamSessionQuestions]  
    @UserExamSessionId BIGINT  
AS  
BEGIN  
    SET NOCOUNT ON;  

    -- Get basic question information with choices (exclude ordering & pairing)
    -- Explicitly mask IsCorrect as 0 to prevent answer disclosure in exam session
    SELECT   
        esq.SessionQuestionId,  
        esq.QuestionId,  
        q.TopicId,  
        ISNULL(st.TopicName, 'General') AS TopicName,  
        ISNULL(s.SubjectName, 'General') AS SubjectName,  
        esq.QuestionTextEnglish,  
        esq.QuestionTextHindi,  
        esq.AdditionalTextEnglish,  
        esq.AdditionalTextHindi,  
        esq.QuestionTypeId,
        esq.IsMultiSelect,
        qt.TypeName AS QuestionTypeName,  
        qt.IsObjective,  
        esq.Marks,  
        esq.NegativeMarks,  
        esq.SortOrder,  
        esc.SessionChoiceId,  
        esc.ChoiceId,  
        esc.ChoiceTextEnglish,  
        esc.ChoiceTextHindi,  
        CAST(0 AS BIT) AS IsCorrect,  -- Anti-cheating: Never disclose correct choice in session
        NULL AS SessionOrderId,  
        NULL AS ItemText,  
        NULL AS CorrectOrder,  
        NULL AS SessionPairId,  
        NULL AS LeftText,  
        NULL AS RightText,  
        1 AS QuestionTypeOrder  
    FROM dbo.ExamSessionQuestion esq  
    INNER JOIN dbo.Question q   
        ON esq.QuestionId = q.QuestionId  
    INNER JOIN dbo.QuestionType qt  
        ON esq.QuestionTypeId = qt.QuestionTypeId  
    LEFT JOIN dbo.SubjectTopic st   
        ON q.TopicId = st.TopicId  
    LEFT JOIN dbo.Subject s   
        ON st.SubjectId = s.SubjectId  
    LEFT JOIN dbo.ExamSessionChoice esc   
        ON esq.SessionQuestionId = esc.SessionQuestionId  
    WHERE esq.UserExamSessionId = @UserExamSessionId  
      AND qt.TypeName NOT IN ('Ordering', 'Matching')

    UNION ALL  

    -- Get ordering questions  
    SELECT   
        esq.SessionQuestionId,  
        esq.QuestionId,  
        q.TopicId,  
        ISNULL(st.TopicName, 'General') AS TopicName,  
        ISNULL(s.SubjectName, 'General') AS SubjectName,  
        esq.QuestionTextEnglish,  
        esq.QuestionTextHindi,  
        esq.AdditionalTextEnglish,  
        esq.AdditionalTextHindi,  
        esq.QuestionTypeId,  
        esq.IsMultiSelect,
        qt.TypeName AS QuestionTypeName,  
        qt.IsObjective,  
        esq.Marks,  
        esq.NegativeMarks,  
        esq.SortOrder,  
        NULL AS SessionChoiceId,  
        NULL AS ChoiceId,  
        NULL AS ChoiceTextEnglish,  
        NULL AS ChoiceTextHindi,  
        CAST(0 AS BIT) AS IsCorrect,  
        eso.SessionOrderId,  
        eso.ItemText,  
        eso.CorrectOrder,  
        NULL AS SessionPairId,  
        NULL AS LeftText,  
        NULL AS RightText,  
        2 AS QuestionTypeOrder  
    FROM dbo.ExamSessionQuestion esq  
    INNER JOIN dbo.Question q   
        ON esq.QuestionId = q.QuestionId  
    INNER JOIN dbo.QuestionType qt  
        ON esq.QuestionTypeId = qt.QuestionTypeId  
    LEFT JOIN dbo.SubjectTopic st   
        ON q.TopicId = st.TopicId  
    LEFT JOIN dbo.Subject s   
        ON st.SubjectId = s.SubjectId  
    INNER JOIN dbo.ExamSessionQuestionOrder eso  
        ON esq.SessionQuestionId = eso.SessionQuestionId  
    WHERE esq.UserExamSessionId = @UserExamSessionId  

    UNION ALL  

    -- Get pairing questions  
    SELECT   
        esq.SessionQuestionId,  
        esq.QuestionId,  
        q.TopicId,  
        ISNULL(st.TopicName, 'General') AS TopicName,  
        ISNULL(s.SubjectName, 'General') AS SubjectName,  
        esq.QuestionTextEnglish,  
        esq.QuestionTextHindi,  
        esq.AdditionalTextEnglish,  
        esq.AdditionalTextHindi,  
        esq.QuestionTypeId,  
        esq.IsMultiSelect,
        qt.TypeName AS QuestionTypeName,  
        qt.IsObjective,  
        esq.Marks,  
        esq.NegativeMarks,  
        esq.SortOrder,  
        NULL AS SessionChoiceId,  
        NULL AS ChoiceId,  
        NULL AS ChoiceTextEnglish,  
        NULL AS ChoiceTextHindi,  
        CAST(0 AS BIT) AS IsCorrect,  
        NULL AS SessionOrderId,  
        NULL AS ItemText,  
        NULL AS CorrectOrder,  
        esp.SessionPairId,  
        esp.LeftText,  
        esp.RightText,  
        3 AS QuestionTypeOrder  
    FROM dbo.ExamSessionQuestion esq  
    INNER JOIN dbo.Question q   
        ON esq.QuestionId = q.QuestionId  
    INNER JOIN dbo.QuestionType qt  
        ON esq.QuestionTypeId = qt.QuestionTypeId  
    LEFT JOIN dbo.SubjectTopic st   
        ON q.TopicId = st.TopicId  
    LEFT JOIN dbo.Subject s   
        ON st.SubjectId = s.SubjectId  
    INNER JOIN dbo.ExamSessionQuestionPair esp  
        ON esq.SessionQuestionId = esp.SessionQuestionId  
    WHERE esq.UserExamSessionId = @UserExamSessionId  
    ORDER BY SortOrder, SessionQuestionId, QuestionTypeOrder;
END
GO
CREATE OR ALTER PROCEDURE [dbo].[_sp_SubmitExamSession] \n @UserExamSessionId BIGINT,\n @SubmitTime DATETIME\nAS\nBEGIN\n    SET NOCOUNT ON;\n    UPDATE dbo.UserExamSession WITH (UPDLOCK)\n    SET Status = 'Submit', SubmitTime = @SubmitTime\n    WHERE UserExamSessionId = @UserExamSessionId AND Status != 'Submit';\n    SELECT @@ROWCOUNT;\nEND\nGO
