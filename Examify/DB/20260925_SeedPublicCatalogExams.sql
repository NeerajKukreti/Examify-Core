-- ============================================================================
-- Examify Public Catalog Seed Data
-- Adds two high-value Platform Certification / Practice Exams
-- 1. C# & .NET Core Engineering Assessment
-- 2. Azure Cloud Architecture & Fundamentals
-- ============================================================================

SET NOCOUNT ON;

DECLARE @InstId INT = 1;
DECLARE @TopicId INT = 1;

-- ----------------------------------------------------------------------------
-- 1. C# & .NET Core Engineering Assessment
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Exam WHERE ExamName = 'C# & .NET Core Assessment')
BEGIN
    DECLARE @ExamId1 INT;
    INSERT INTO dbo.Exam (
        InstituteId, ExamName, Description, Image, IsActive, CreatedBy, CreatedDate,
        DurationMinutes, TotalQuestions, Instructions, ExamType, CutOffPercentage,
        IsPublished, IsPublic
    )
    VALUES (
        @InstId,
        'C# & .NET Core Assessment',
        'Demonstrate mastery of modern C#, memory management, CLR runtime, async programming, and ASP.NET Core architecture.',
        NULL, 1, 1, GETDATE(),
        20, 5,
        '<p><strong>Instructions:</strong> This exam contains 5 multiple-choice questions assessing advanced C# concepts, runtime architecture, and memory management. Passing score is 60%. Answer all questions before submitting.</p>',
        'Practice', 60.00,
        1, 1
    );

    SET @ExamId1 = SCOPE_IDENTITY();

    -- Question 1
    DECLARE @Q1 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>In modern .NET, what is the primary architectural difference between <code>Task.Run</code> and <code>Task.Factory.StartNew</code>?</p>', '', '', '', 'Task.Run is a convenience wrapper over Task.Factory.StartNew that passes default scheduler and DenyChildAttach flags.', 1, 0, 1, GETDATE(), 0, 'Medium');
    SET @Q1 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q1, '<p>Task.Run is a streamlined wrapper configuring TaskScheduler.Default and TaskCreationOptions.DenyChildAttach</p>', '', 1),
    (@Q1, '<p>Task.Run creates an unmanaged OS thread, whereas StartNew uses the ThreadPool</p>', '', 0),
    (@Q1, '<p>Task.Run can only execute synchronous action delegates</p>', '', 0),
    (@Q1, '<p>There is no behavioral difference between them</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId1, @Q1, 1.0, 0.0, 1);

    -- Question 2
    DECLARE @Q2 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>Which generation or heap in the .NET CLR Garbage Collector manages objects 85,000 bytes or larger by default?</p>', '', '', '', 'Objects 85,000 bytes or larger are allocated directly on the Large Object Heap (LOH) to avoid expensive compactions.', 1, 0, 1, GETDATE(), 0, 'Medium');
    SET @Q2 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q2, '<p>Generation 0</p>', '', 0),
    (@Q2, '<p>Generation 1</p>', '', 0),
    (@Q2, '<p>Large Object Heap (LOH)</p>', '', 1),
    (@Q2, '<p>Ephemeral Generation</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId1, @Q2, 1.0, 0.0, 2);

    -- Question 3
    DECLARE @Q3 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>In ASP.NET Core dependency injection, which service lifetime ensures a single service instance per HTTP request context?</p>', '', '', '', 'Scoped lifetime creates a new instance once per client request (connection) and disposes it at the end of the request.', 1, 0, 1, GETDATE(), 0, 'Easy');
    SET @Q3 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q3, '<p>Transient</p>', '', 0),
    (@Q3, '<p>Scoped</p>', '', 1),
    (@Q3, '<p>Singleton</p>', '', 0),
    (@Q3, '<p>ThreadStatic</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId1, @Q3, 1.0, 0.0, 3);

    -- Question 4
    DECLARE @Q4 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>What is the critical risk when throwing an unhandled exception inside an <code>async void</code> method?</p>', '', '', '', 'async void methods cannot be awaited and have no Task object to capture errors. Unhandled exceptions are posted directly to the SynchronizationContext, terminating the process.', 1, 0, 1, GETDATE(), 0, 'Hard');
    SET @Q4 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q4, '<p>The exception is captured and returned via the caller awaiter Task</p>', '', 0),
    (@Q4, '<p>The exception is posted to the active SynchronizationContext and commonly crashes the application process</p>', '', 1),
    (@Q4, '<p>The exception is swallowed silently without logging</p>', '', 0),
    (@Q4, '<p>The CLR automatically triggers an immediate retry loop</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId1, @Q4, 1.0, 0.0, 4);

    -- Question 5
    DECLARE @Q5 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>Which property accessor keyword allows immutability after object initialization in C# 9+?</p>', '', '', '', 'The init accessor restricts property assignment to object construction or initializers only.', 1, 0, 1, GETDATE(), 0, 'Easy');
    SET @Q5 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q5, '<p>readonly</p>', '', 0),
    (@Q5, '<p>const</p>', '', 0),
    (@Q5, '<p>init</p>', '', 1),
    (@Q5, '<p>immutable</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId1, @Q5, 1.0, 0.0, 5);

    PRINT 'Created Exam: C# & .NET Core Assessment (ExamId: ' + CAST(@ExamId1 AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT 'Exam "C# & .NET Core Assessment" already exists.';
END

-- ----------------------------------------------------------------------------
-- 2. Azure Cloud Architecture & Fundamentals
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Exam WHERE ExamName = 'Azure Cloud Architecture & Fundamentals')
BEGIN
    DECLARE @ExamId2 INT;
    INSERT INTO dbo.Exam (
        InstituteId, ExamName, Description, Image, IsActive, CreatedBy, CreatedDate,
        DurationMinutes, TotalQuestions, Instructions, ExamType, CutOffPercentage,
        IsPublished, IsPublic
    )
    VALUES (
        @InstId,
        'Azure Cloud Architecture & Fundamentals',
        'Verify your understanding of cloud architecture concepts, core Azure compute, networking, security policies, and fault tolerance.',
        NULL, 1, 1, GETDATE(),
        20, 5,
        '<p><strong>Instructions:</strong> This exam tests your cloud architecture fundamentals. 5 multiple-choice questions. Passing score is 70%. Good luck!</p>',
        'Practice', 70.00,
        1, 1
    );

    SET @ExamId2 = SCOPE_IDENTITY();

    -- Question 1
    DECLARE @Q6 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>Which cloud service category gives the customer maximum management control over the OS, storage, and networking layers?</p>', '', '', '', 'IaaS provides raw compute VMs and networks where the customer maintains the OS and runtime.', 1, 0, 1, GETDATE(), 0, 'Easy');
    SET @Q6 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q6, '<p>Software as a Service (SaaS)</p>', '', 0),
    (@Q6, '<p>Platform as a Service (PaaS)</p>', '', 0),
    (@Q6, '<p>Infrastructure as a Service (IaaS)</p>', '', 1),
    (@Q6, '<p>Function as a Service (FaaS)</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId2, @Q6, 1.0, 0.0, 1);

    -- Question 2
    DECLARE @Q7 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>What Azure architectural feature provides physically separate datacenter facilities with independent power, cooling, and network within a single region?</p>', '', '', '', 'Availability Zones are physically distinct datacenters within an Azure region delivering high availability and zone redundancy.', 1, 0, 1, GETDATE(), 0, 'Medium');
    SET @Q7 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q7, '<p>Availability Sets</p>', '', 0),
    (@Q7, '<p>Availability Zones</p>', '', 1),
    (@Q7, '<p>Resource Groups</p>', '', 0),
    (@Q7, '<p>Geo-Redundant Peering</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId2, @Q7, 1.0, 0.0, 2);

    -- Question 3
    DECLARE @Q8 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>Which fully managed relational database PaaS service offers automatic scaling, intelligent query tuning, and 99.99% SLA in Azure?</p>', '', '', '', 'Azure SQL Database is the leading relational PaaS database engine with built-in high availability.', 1, 0, 1, GETDATE(), 0, 'Easy');
    SET @Q8 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q8, '<p>Azure SQL Database</p>', '', 1),
    (@Q8, '<p>Azure Cosmos DB (Table API)</p>', '', 0),
    (@Q8, '<p>Azure Blob Storage</p>', '', 0),
    (@Q8, '<p>Azure Cache for Redis</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId2, @Q8, 1.0, 0.0, 3);

    -- Question 4
    DECLARE @Q9 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>In Azure governance, what service is used to enforce organizational standards, prevent non-compliant resource deployments, and assess compliance?</p>', '', '', '', 'Azure Policy evaluates resources in Azure by comparing their properties to business rules defined in JSON format.', 1, 0, 1, GETDATE(), 0, 'Medium');
    SET @Q9 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q9, '<p>Azure Policy</p>', '', 1),
    (@Q9, '<p>Azure Key Vault</p>', '', 0),
    (@Q9, '<p>Azure Bastion</p>', '', 0),
    (@Q9, '<p>Azure Service Health</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId2, @Q9, 1.0, 0.0, 4);

    -- Question 5
    DECLARE @Q10 INT;
    INSERT INTO dbo.Question (InstituteId, TopicId, QuestionEnglish, QuestionHindi, AdditionalTextEnglish, AdditionalTextHindi, Explanation, QuestionTypeId, IsDeleted, CreatedBy, CreatedDate, IsMultiSelect, DifficultyLevel)
    VALUES (@InstId, @TopicId, '<p>Which architecture design principle balances traffic across redundant backend resources to prevent single point of failure?</p>', '', '', '', 'Load balancing distributes incoming traffic evenly across healthy targets to ensure uninterrupted availability.', 1, 0, 1, GETDATE(), 0, 'Easy');
    SET @Q10 = SCOPE_IDENTITY();
    INSERT INTO dbo.QuestionChoice (QuestionId, ChoiceTextEnglish, ChoiceTextHindi, IsCorrect) VALUES
    (@Q10, '<p>Load Balancing & Redundancy</p>', '', 1),
    (@Q10, '<p>Data Sharding</p>', '', 0),
    (@Q10, '<p>Event Sourcing</p>', '', 0),
    (@Q10, '<p>Write-Through Caching</p>', '', 0);
    INSERT INTO dbo.ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder) VALUES (@ExamId2, @Q10, 1.0, 0.0, 5);

    PRINT 'Created Exam: Azure Cloud Architecture & Fundamentals (ExamId: ' + CAST(@ExamId2 AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT 'Exam "Azure Cloud Architecture & Fundamentals" already exists.';
END
