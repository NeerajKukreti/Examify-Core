using Microsoft.AspNetCore.Mvc.Rendering;

namespace DataModel
{
    public class SubjectTopic
    {
        public int TopicId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string Topic { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SubjectModel
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string Image { get; set; }
        public int NoOfQuestion { get; set; }

        public string Topics { get; set; }

        public List<SelectListItem> TopicList { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
