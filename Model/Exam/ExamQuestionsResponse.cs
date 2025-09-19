using System.Collections.Generic;

namespace DataModel
{
    public class ExamQuestionsResponse
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int DurationMinutes { get; set; } 
        public int TotalQuestions { get; set; } // Added for completeness
        public string Instructions { get; set; } // Added for completeness
        public string ExamType { get; set; } // Added for completeness
        public decimal? CutOffPercentage { get; set; } // Added for completeness
        public List<ExamSectionModel> Sections { get; set; } = new List<ExamSectionModel>();
        public string SessionId { get; set; }  
    }
}