namespace Model.DTO
{
    public class StatsDTO
    {
        public int TotalQuestions { get; set; }
        public List<QuestionTypeStatsDTO> QuestionsByType { get; set; }
        public List<DifficultyStatsDTO> QuestionsByDifficulty { get; set; }
        public List<SubjectStatsDTO> SubjectStats { get; set; }
        public List<SubjectQuestionStatsDTO> SubjectQuestionStats { get; set; }
        public int TotalClasses { get; set; }
    }

    public class QuestionTypeStatsDTO
    {
        public string TypeName { get; set; }
        public int TotalByType { get; set; }
    }

    public class DifficultyStatsDTO
    {
        public string DifficultyLevel { get; set; }
        public int TotalByDifficulty { get; set; }
    }

    public class SubjectStatsDTO
    {
        public string SubjectName { get; set; }
        public int TotalTopics { get; set; }
    }

    public class SubjectQuestionStatsDTO
    {
        public string SubjectName { get; set; }
        public int TotalQuestions { get; set; }
    }
}
