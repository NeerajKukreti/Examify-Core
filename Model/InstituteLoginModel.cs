using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataModel
{
    public class InstituteLoginModel
    {
        public string PrimaryContact { get; set; }
        public string Password { get; set; }
        public int StatusCode { get; set; }  // Return Institute Id when success
        public string InstituteName { get; set; }
    }
}