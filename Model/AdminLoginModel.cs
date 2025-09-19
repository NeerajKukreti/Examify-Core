using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataModel
{
    public class AdminLoginModel
    {
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public int StatusCode { get; set; }
        public string AdminName { get; set; }
    }
}