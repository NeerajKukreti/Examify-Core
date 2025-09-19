using System;

namespace DataModel
{
    public class ViolationModel
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string Timestamp { get; set; }
        public long TimeFromStart { get; set; }
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public int? UserId { get; set; }
        public int? ExamId { get; set; }
        public int? SessionId { get; set; }
    }
}