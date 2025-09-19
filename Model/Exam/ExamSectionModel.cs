using DataModel;
using System.Collections.Generic;

namespace DataModel
{
    public class ExamSectionModel
    {
        public string SectionName { get; set; } = string.Empty;
        public List<ExamSessionQuestionModel> Questions { get; set; } = new List<ExamSessionQuestionModel>();
    }
}