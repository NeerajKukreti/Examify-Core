-- Randomization Preset Tables

CREATE TABLE [dbo].[RandomizationPreset](
    [PresetId] INT PRIMARY KEY IDENTITY(1,1),
    [PresetName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [InstituteId] INT NOT NULL,
    [CreatedBy] INT,
    [CreatedDate] DATETIME DEFAULT GETDATE(),
    [IsActive] BIT DEFAULT 1,
    CONSTRAINT FK_RandomizationPreset_Institute FOREIGN KEY ([InstituteId]) REFERENCES [Institute]([InstituteId]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[RandomizationPresetDetail](
    [PresetDetailId] INT PRIMARY KEY IDENTITY(1,1),
    [PresetId] INT NOT NULL,
    [SubjectId] INT,
    [TopicId] INT,
    [DifficultyLevel] NVARCHAR(20),
    [QuestionTypeId] INT,
    [PickCount] INT NOT NULL,
    CONSTRAINT FK_PresetDetail_Preset FOREIGN KEY ([PresetId]) REFERENCES [RandomizationPreset]([PresetId]) ON DELETE CASCADE,
    CONSTRAINT FK_PresetDetail_Subject FOREIGN KEY ([SubjectId]) REFERENCES [Subject]([SubjectId]),
    CONSTRAINT FK_PresetDetail_Topic FOREIGN KEY ([TopicId]) REFERENCES [SubjectTopic]([TopicId]),
    CONSTRAINT FK_PresetDetail_QuestionType FOREIGN KEY ([QuestionTypeId]) REFERENCES [QuestionType]([QuestionTypeId])
);

CREATE INDEX IX_RandomizationPreset_Institute ON [RandomizationPreset]([InstituteId]);
CREATE INDEX IX_PresetDetail_Preset ON [RandomizationPresetDetail]([PresetId]);
