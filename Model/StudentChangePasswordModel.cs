using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataModel
{
    public class StudentChangePasswordModel
    {
        public int StudentId { get; set; }  
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public int StatusCode { get; set; }  // Return 0 for Wrong Password and 1 for Success 
    }
}