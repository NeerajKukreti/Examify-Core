using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DataModel
{

    [ValidateNever]
    public class QuestionModel
    {
        public int? QuestionId { get; set; } = 0;
        public int TopicId { get; set; }
        public int SubjectId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string QuestionEnglish { get; set; } = string.Empty;
        public string QuestionHindi { get; set; } = string.Empty;
        public string AdditionalTextEnglish { get; set; } = string.Empty;
        public string AdditionalTextHindi { get; set; } = string.Empty;
        public string? Explanation { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int QuestionTypeId { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsMultiSelect { get; set; }
        public List<OptionModel> Options { get; set; } = new List<OptionModel>();
        public List<PairModel> Pairs { get; set; } = new List<PairModel>(); // For Pairing questions
        public List<OrderModel> Orders { get; set; } = new List<OrderModel>(); // For Ordering questions
    }

    public class OptionModel
    {
        public int? ChoiceId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool? IsCorrect { get; set; }
    }

    public class PairModel
    {
        public long? PairId { get; set; } // For DB mapping
        public string LeftText { get; set; } = string.Empty;
        public string RightText { get; set; } = string.Empty;
    }

    public class OrderModel
    {
        public long? OrderId { get; set; } // For DB mapping
        public string ItemText { get; set; } = string.Empty;
        public int? CorrectOrder { get; set; } // For ordering
    }
}