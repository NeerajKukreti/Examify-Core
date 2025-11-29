using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Model.DTO;
using System.Data;

namespace DAL.Repository
{
    public interface IRandomizationPresetRepository
    {
        Task<IEnumerable<RandomizationPresetDTO>> GetAllPresetsAsync(int instituteId);
        Task<RandomizationPresetDTO?> GetPresetByIdAsync(int presetId);
        Task<int> CreatePresetAsync(RandomizationPresetDTO preset);
        Task<bool> UpdatePresetAsync(RandomizationPresetDTO preset);
        Task<bool> DeletePresetAsync(int presetId);
        Task<PresetPreviewDTO> PreviewPresetAsync(int presetId, int instituteId);
        Task<List<int>> ExecutePresetAsync(PresetExecutionDTO execution);
    }

    public class RandomizationPresetRepository : IRandomizationPresetRepository
    {
        private readonly IConfiguration _config;
        public RandomizationPresetRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection() => 
            new SqlConnection(_config.GetConnectionString("DefaultConnection"));


        public async Task<IEnumerable<RandomizationPresetDTO>> GetAllPresetsAsync(int instituteId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT p.*, 
                       d.PresetDetailId, d.SubjectId, d.TopicId, d.DifficultyLevel, 
                       d.QuestionTypeId, d.PickCount,
                       s.SubjectName, st.TopicName, qt.TypeName
                FROM RandomizationPreset p
                LEFT JOIN RandomizationPresetDetail d ON p.PresetId = d.PresetId
                LEFT JOIN Subject s ON d.SubjectId = s.SubjectId
                LEFT JOIN SubjectTopic st ON d.TopicId = st.TopicId
                LEFT JOIN QuestionType qt ON d.QuestionTypeId = qt.QuestionTypeId
                WHERE p.InstituteId = @InstituteId AND p.IsActive = 1
                ORDER BY p.CreatedDate DESC";

            var presetDict = new Dictionary<int, RandomizationPresetDTO>();
            
            await connection.QueryAsync<RandomizationPresetDTO, RandomizationPresetDetailDTO, RandomizationPresetDTO>(
                sql,
                (preset, detail) =>
                {
                    if (!presetDict.TryGetValue(preset.PresetId, out var presetEntry))
                    {
                        presetEntry = preset;
                        presetEntry.Details = new List<RandomizationPresetDetailDTO>();
                        presetDict.Add(preset.PresetId, presetEntry);
                    }
                    if (detail != null && detail.PresetDetailId > 0)
                        presetEntry.Details.Add(detail);
                    return presetEntry;
                },
                new { InstituteId = instituteId },
                splitOn: "PresetDetailId"
            );

            return presetDict.Values;
        }

        public async Task<RandomizationPresetDTO?> GetPresetByIdAsync(int presetId)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT p.*, 
                       d.PresetDetailId, d.SubjectId, d.TopicId, d.DifficultyLevel, 
                       d.QuestionTypeId, d.PickCount,
                       s.SubjectName, st.TopicName, qt.TypeName
                FROM RandomizationPreset p
                LEFT JOIN RandomizationPresetDetail d ON p.PresetId = d.PresetId
                LEFT JOIN Subject s ON d.SubjectId = s.SubjectId
                LEFT JOIN SubjectTopic st ON d.TopicId = st.TopicId
                LEFT JOIN QuestionType qt ON d.QuestionTypeId = qt.QuestionTypeId
                WHERE p.PresetId = @PresetId";

            RandomizationPresetDTO? preset = null;
            
            await connection.QueryAsync<RandomizationPresetDTO, RandomizationPresetDetailDTO, RandomizationPresetDTO>(
                sql,
                (p, detail) =>
                {
                    preset ??= p;
                    preset.Details ??= new List<RandomizationPresetDetailDTO>();
                    if (detail != null && detail.PresetDetailId > 0)
                        preset.Details.Add(detail);
                    return preset;
                },
                new { PresetId = presetId },
                splitOn: "PresetDetailId"
            );

            return preset;
        }

        public async Task<int> CreatePresetAsync(RandomizationPresetDTO preset)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var presetId = await connection.ExecuteScalarAsync<int>(@"
                    INSERT INTO RandomizationPreset (PresetName, Description, InstituteId, CreatedBy, IsActive)
                    VALUES (@PresetName, @Description, @InstituteId, @CreatedBy, @IsActive);
                    SELECT CAST(SCOPE_IDENTITY() as int)", preset, transaction);

                foreach (var detail in preset.Details)
                {
                    detail.PresetId = presetId;
                    await connection.ExecuteAsync(@"
                        INSERT INTO RandomizationPresetDetail (PresetId, SubjectId, TopicId, DifficultyLevel, QuestionTypeId, PickCount)
                        VALUES (@PresetId, @SubjectId, @TopicId, @DifficultyLevel, @QuestionTypeId, @PickCount)", 
                        detail, transaction);
                }

                transaction.Commit();
                return presetId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdatePresetAsync(RandomizationPresetDTO preset)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(@"
                    UPDATE RandomizationPreset 
                    SET PresetName = @PresetName, Description = @Description, IsActive = @IsActive
                    WHERE PresetId = @PresetId", preset, transaction);

                await connection.ExecuteAsync("DELETE FROM RandomizationPresetDetail WHERE PresetId = @PresetId", 
                    new { preset.PresetId }, transaction);

                foreach (var detail in preset.Details)
                {
                    detail.PresetId = preset.PresetId;
                    await connection.ExecuteAsync(@"
                        INSERT INTO RandomizationPresetDetail (PresetId, SubjectId, TopicId, DifficultyLevel, QuestionTypeId, PickCount)
                        VALUES (@PresetId, @SubjectId, @TopicId, @DifficultyLevel, @QuestionTypeId, @PickCount)", 
                        detail, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeletePresetAsync(int presetId)
        {
            using var connection = CreateConnection();
            var result = await connection.ExecuteAsync(
                "UPDATE RandomizationPreset SET IsActive = 0 WHERE PresetId = @PresetId", 
                new { PresetId = presetId });
            return result > 0;
        }

        public async Task<PresetPreviewDTO> PreviewPresetAsync(int presetId, int instituteId)
        {
            var preset = await GetPresetByIdAsync(presetId);
            if (preset == null) return new PresetPreviewDTO();

            using var connection = CreateConnection();
            var preview = new PresetPreviewDTO();

            foreach (var detail in preset.Details)
            {
                var sql = @"
                    SELECT COUNT(*) 
                    FROM Question q
                    WHERE q.InstituteId = @InstituteId
                    AND (@SubjectId IS NULL OR q.SubjectId = @SubjectId)
                    AND (@TopicId IS NULL OR q.TopicId = @TopicId)
                    AND (@DifficultyLevel IS NULL OR q.DifficultyLevel = @DifficultyLevel)
                    AND (@QuestionTypeId IS NULL OR q.QuestionTypeId = @QuestionTypeId)";

                var availableCount = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    InstituteId = instituteId,
                    detail.SubjectId,
                    detail.TopicId,
                    detail.DifficultyLevel,
                    detail.QuestionTypeId
                });

                preview.Details.Add(new PresetPreviewDetailDTO
                {
                    SubjectName = detail.SubjectName,
                    TopicName = detail.TopicName,
                    DifficultyLevel = detail.DifficultyLevel,
                    RequestedCount = detail.PickCount,
                    AvailableCount = availableCount,
                    HasEnough = availableCount >= detail.PickCount
                });

                preview.TotalQuestions += Math.Min(detail.PickCount, availableCount);
            }

            return preview;
        }

        public async Task<List<int>> ExecutePresetAsync(PresetExecutionDTO execution)
        {
            var preset = await GetPresetByIdAsync(execution.PresetId);
            if (preset == null) return new List<int>();

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var addedQuestionIds = new List<int>();
                var sortOrder = await connection.ExecuteScalarAsync<int>(
                    "SELECT ISNULL(MAX(SortOrder), 0) FROM ExamQuestion WHERE ExamId = @ExamId",
                    new { execution.ExamId }, transaction);

                foreach (var detail in preset.Details)
                {
                    var sql = @"
                        SELECT TOP (@PickCount) q.QuestionId
                        FROM Question q
                        LEFT JOIN ExamQuestion eq ON q.QuestionId = eq.QuestionId AND eq.ExamId = @ExamId
                        WHERE eq.QuestionId IS NULL
                        AND (@SubjectId IS NULL OR q.SubjectId = @SubjectId)
                        AND (@TopicId IS NULL OR q.TopicId = @TopicId)
                        AND (@DifficultyLevel IS NULL OR q.DifficultyLevel = @DifficultyLevel)
                        AND (@QuestionTypeId IS NULL OR q.QuestionTypeId = @QuestionTypeId)
                        ORDER BY NEWID()";

                    var questionIds = await connection.QueryAsync<int>(sql, new
                    {
                        execution.ExamId,
                        detail.PickCount,
                        detail.SubjectId,
                        detail.TopicId,
                        detail.DifficultyLevel,
                        detail.QuestionTypeId
                    }, transaction);

                    foreach (var qId in questionIds)
                    {
                        await connection.ExecuteAsync(@"
                            INSERT INTO ExamQuestion (ExamId, QuestionId, Marks, NegativeMarks, SortOrder)
                            VALUES (@ExamId, @QuestionId, @Marks, @NegativeMarks, @SortOrder)",
                            new
                            {
                                execution.ExamId,
                                QuestionId = qId,
                                Marks = execution.DefaultMarks,
                                NegativeMarks = execution.DefaultNegativeMarks,
                                SortOrder = ++sortOrder
                            }, transaction);
                        addedQuestionIds.Add(qId);
                    }
                }

                await connection.ExecuteAsync(
                    "UPDATE Exam SET TotalQuestions = (SELECT COUNT(*) FROM ExamQuestion WHERE ExamId = @ExamId) WHERE ExamId = @ExamId",
                    new { execution.ExamId }, transaction);

                transaction.Commit();
                return addedQuestionIds;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
