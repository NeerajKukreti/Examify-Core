-- ==============================================================================
-- Migration: 20260925_PersonalTenant_PublicCatalog.sql
-- Description: Adds Personal Tenant support (TenantType, OwnerUserId, PlanType)
--              and Public Platform Exams (IsPublic) with monthly free usage quota tracking.
-- ==============================================================================

USE [db_exam];
GO

-- 1. Add TenantType, OwnerUserId, PlanType to dbo.Institute
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Institute' AND COLUMN_NAME = 'TenantType')
BEGIN
    ALTER TABLE dbo.Institute ADD TenantType INT NOT NULL CONSTRAINT DF_Institute_TenantType DEFAULT 2; -- 1 = Personal, 2 = Institute
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Institute' AND COLUMN_NAME = 'OwnerUserId')
BEGIN
    ALTER TABLE dbo.Institute ADD OwnerUserId INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Institute' AND COLUMN_NAME = 'PlanType')
BEGIN
    ALTER TABLE dbo.Institute ADD PlanType NVARCHAR(50) NOT NULL CONSTRAINT DF_Institute_PlanType DEFAULT 'Free';
END
GO

-- 2. Add IsPublic to dbo.Exam
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Exam' AND COLUMN_NAME = 'IsPublic')
BEGIN
    ALTER TABLE dbo.Exam ADD IsPublic BIT NOT NULL CONSTRAINT DF_Exam_IsPublic DEFAULT 0;
END
GO

-- 3. Create Index on Public Exams
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Exam_IsPublic' AND object_id = OBJECT_ID('dbo.Exam'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Exam_IsPublic 
    ON dbo.Exam (IsPublic, IsPublished, IsActive) 
    INCLUDE (ExamName, Description, Image, DurationMinutes, TotalQuestions, CutOffPercentage, Instructions);
END
GO

-- 4. Update dbo._sp_GetUser to support TenantType and PlanType
CREATE OR ALTER PROCEDURE [dbo].[_sp_GetUser]      
    @Username NVARCHAR(200)      
AS      
BEGIN      
    SET NOCOUNT ON;      
      
    SELECT U.Username,      
        CASE 
            WHEN S.StudentId IS NOT NULL THEN S.StudentId      
            WHEN I.InstituteId IS NOT NULL THEN I.InstituteId  
            ELSE U.UserId
        END AS UserId,      
        CASE       
            WHEN S.StudentId IS NOT NULL THEN S.StudentName      
            WHEN I.InstituteId IS NOT NULL THEN I.InstituteName
            ELSE U.Username
        END AS [FullName],
        CASE       
            WHEN S.StudentId IS NOT NULL THEN S.InstituteId      
            WHEN I.InstituteId IS NOT NULL THEN I.InstituteId
            ELSE 0
        END AS InstituteId,
        CASE       
            WHEN S.StudentId IS NOT NULL THEN 'Student'      
            WHEN I.InstituteId IS NOT NULL THEN 'Institute'
            ELSE U.Role
        END AS Role,      
        CASE       
            WHEN S.StudentId IS NOT NULL THEN S.Email      
            WHEN I.InstituteId IS NOT NULL THEN I.Email
            ELSE ''
        END AS Email,
        U.PasswordHash,
        ISNULL(Inst.TenantType, 2) AS TenantType,
        ISNULL(Inst.PlanType, 'Free') AS PlanType
    FROM Users U      
    LEFT JOIN Student S ON U.UserId = S.UserId      
    LEFT JOIN Institute I ON U.UserId = I.UserId
    LEFT JOIN Institute Inst ON Inst.InstituteId = CASE 
        WHEN S.StudentId IS NOT NULL THEN S.InstituteId 
        WHEN I.InstituteId IS NOT NULL THEN I.InstituteId 
        ELSE 0 
    END
    WHERE U.Username = @Username;      
END;
GO

-- 5. Stored Procedure to Get Public Exam Catalog
CREATE OR ALTER PROCEDURE [dbo].[_sp_GetPublicExams]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Result set 1: Public active published exams
    SELECT 
        ExamId, 
        InstituteId, 
        ExamName, 
        Description, 
        Image, 
        DurationMinutes, 
        TotalQuestions,          
        Instructions, 
        ExamType, 
        IsActive, 
        IsPublished, 
        IsPublic,
        CutOffPercentage
    FROM dbo.Exam
    WHERE IsPublic = 1 AND IsPublished = 1 AND IsActive = 1
    ORDER BY CreatedDate DESC;

    -- Result set 2: Empty/Associated class mapping for consistency
    SELECT ExamId, ClassId 
    FROM dbo.ExamClass 
    WHERE ExamId IN (
        SELECT ExamId 
        FROM dbo.Exam 
        WHERE IsPublic = 1 AND IsPublished = 1 AND IsActive = 1
    );
END;
GO

-- 6. Update dbo._sp_GetAllExams to return IsPublic and respect public exam visibility
CREATE OR ALTER PROCEDURE [dbo].[_sp_GetAllExams]          
    @InstituteId INT = 0,    
    @ExamId INT = NULL,
    @IncludePublic BIT = 0        
AS          
BEGIN        
    SET NOCOUNT ON;

    IF @ExamId IS NULL        
    BEGIN        
        SELECT 
            ExamId, 
            InstituteId, 
            ExamName, 
            Description, 
            Image, 
            DurationMinutes, 
            TotalQuestions,          
            Instructions, 
            ExamType, 
            IsActive, 
            IsPublished,
            IsPublic,
            CutOffPercentage          
        FROM dbo.Exam    
        WHERE (@InstituteId > 0 AND InstituteId = @InstituteId)
           OR (@IncludePublic = 1 AND IsPublic = 1 AND IsPublished = 1 AND IsActive = 1)
           OR (@InstituteId = 0 AND IsPublic = 1 AND IsPublished = 1 AND IsActive = 1)
        ORDER BY CreatedDate DESC;      
    END        
    ELSE        
    BEGIN        
        SELECT 
            ExamId, 
            InstituteId, 
            ExamName, 
            Description, 
            Image, 
            DurationMinutes, 
            TotalQuestions,          
            Instructions, 
            ExamType, 
            IsActive, 
            IsPublished,
            IsPublic,
            CutOffPercentage          
        FROM dbo.Exam          
        WHERE ExamId = @ExamId;      
    END;   
   
    SELECT ExamId, ClassId 
    FROM dbo.ExamClass 
    WHERE (@ExamId IS NULL) OR ExamId = @ExamId;   
END;
GO

-- 7. Stored Procedure to count user monthly exam sessions
CREATE OR ALTER PROCEDURE [dbo].[_sp_GetUserMonthlyExamCount]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT COUNT(1)
    FROM dbo.UserExamSession
    WHERE UserId = @UserId
      AND CreatedDate >= DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1);
END;
GO

-- 8. Update dbo._sp_InsertUpdateExam to accept @IsPublic
CREATE OR ALTER PROCEDURE [dbo].[_sp_InsertUpdateExam]
    @classIds dbo.IntList readonly,
    @InstituteId int,
    @ExamId INT = NULL,
    @ExamName NVARCHAR(200),
    @Description NVARCHAR(MAX) = NULL,
    @Image NVARCHAR(200) = NULL,
    @DurationMinutes INT,
    @TotalQuestions INT,
    @Instructions NVARCHAR(MAX) = NULL,
    @ExamType NVARCHAR(50) = NULL,
    @CutOffPercentage DECIMAL(5,2) = NULL,
    @UserId INT = NULL,
    @ModifiedBy INT = NULL,
    @IsPublic BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @ExamId IS NULL OR @ExamId = 0
    BEGIN
        INSERT INTO Exam (
            InstituteId, ExamName, Description, Image, IsActive, CreatedBy, CreatedDate, DurationMinutes, TotalQuestions, 
            Instructions, ExamType, CutOffPercentage, IsPublic
        )
        VALUES (
            @InstituteId, @ExamName, @Description, @Image, 1, @UserId, GETDATE(), @DurationMinutes, @TotalQuestions, 
            @Instructions, @ExamType, @CutOffPercentage, @IsPublic        
        );
        
        SET @ExamId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE Exam
        SET 
            ExamName = @ExamName,
            Description = @Description,
            Image = @Image,
            DurationMinutes = @DurationMinutes,
            TotalQuestions = @TotalQuestions,
            Instructions = @Instructions,
            ExamType = @ExamType,
            CutOffPercentage = @CutOffPercentage,
            ModifiedBy = @UserId,
            ModifiedDate = GETDATE(),
            IsPublic = @IsPublic
        WHERE ExamId = @ExamId;
        
        SELECT @ExamId;
    END

    DELETE FROM ExamClass 
    WHERE Classid NOT IN (SELECT id FROM @classIds)
      AND Examid = @ExamId;

    INSERT INTO ExamClass(ExamId, classid) 
    SELECT @examid, id 
    FROM @classIds 
    WHERE id NOT IN (SELECT classid FROM ExamClass WHERE Examid = @ExamId);

    SELECT @ExamId;
END;
GO
