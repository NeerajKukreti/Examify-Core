namespace DataModel
{
    public class SubjectTopicModel
    {
        public int TopicId { get; set; }
        public int SubjectId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation
        public string SubjectName { get; set; } = string.Empty;
    }
}