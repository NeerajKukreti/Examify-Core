using System;
using System.Collections.Generic;
using DataModel.Exam;

namespace DataModel
{
    public class ExamSessionQuestionModel
    {
        public long SessionQuestionId { get; set; }
        public long UserExamSessionId { get; set; }
        public int QuestionId { get; set; }
        public string TopicName { get; set; }
        public string SubjectName { get; set; }
        public string? QuestionTextEnglish { get; set; }
        public string? QuestionTextHindi { get; set; }
        public string? AdditionalTextEnglish { get; set; }
        public string? AdditionalTextHindi { get; set; }
        public int? QuestionTypeId { get; set; } // Updated to match DB
        public string? QuestionTypeName { get; set; } // For display
        public bool? IsObjective { get; set; } // For display
        public decimal Marks { get; set; }
        public decimal NegativeMarks { get; set; }
        public int? SortOrder { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsMultiSelect { get; set; }
        
        // Navigation properties
        public UserExamSessionModel? UserExamSession { get; set; }
        public ICollection<ExamSessionChoiceModel>? SessionChoices { get; set; }
        public ICollection<ExamSessionQuestionOrderModel>? SessionOrders { get; set; }
        public ICollection<ExamSessionQuestionPairModel>? SessionPairs { get; set; }
    }
}