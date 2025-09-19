using DataModel.Exam;
using System;
using System.Collections.Generic;

namespace DataModel
{
    public class ExamSubmissionModel
    {
        public int ExamId { get; set; }
        public int UserId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int TotalTimeSpent { get; set; }
        public List<ExamResponseSubmissionModel> Responses { get; set; } = new List<ExamResponseSubmissionModel>();
        public string SessionId { get; set; }
    }

    public class ExamResponseSubmissionModel
    {
        public int SessionQuestionId { get; set; }
        public List<int> SessionChoiceIds { get; set; } = new List<int>(); // Used for both single and multi-select
        public int TimeSpent { get; set; }
        public bool IsMarkedForReview { get; set; }
        public string ResponseText { get; set; } // For subjective/pairing
        public List<ExamResponseOrderModel> OrderedItems { get; set; } = new List<ExamResponseOrderModel>();
        public List<ExamResponsePairModel> PairedItems { get; set; } = new List<ExamResponsePairModel>();

    }
}