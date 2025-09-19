using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataModel
{
    public class StudentLoginModel
    {
        public string Mobile { get; set; }
        public string Password { get; set; }
        public int StatusCode { get; set; }
        public string StudentName { get; set; }
    }
}