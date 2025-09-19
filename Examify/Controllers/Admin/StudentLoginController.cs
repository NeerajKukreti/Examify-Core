using DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlineExam.Common;
using OnlineExam.Helpers;
using OnlineExam.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web; 

namespace OnlineExam.Controllers
{
    public class StudentLoginController : Controller
    {
        private readonly IStudentLoginService _studentLoginService;

        public StudentLoginController(IStudentLoginService StudentLoginService)
        {
            _studentLoginService = StudentLoginService;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> index(StudentLoginModel StudentLogin)
        {
            StudentLogin= await _studentLoginService.StudentLoginDetails(StudentLogin);
            if (StudentLogin.StatusCode > 0)
            {
                //FormsAuthentication.SetAuthCookie(StudentLogin.Mobile, false);
                //Session.Add("Student", StudentLogin);
                //Session.Add("Role", "Student");

                return RedirectToAction("index", "dashboard");
            }
            else
            {
                return View("StudentLogin", StudentLogin);
            }
            

        }

        // GET: Admin
        public ActionResult Index()
        {
            return View("StudentLogin", new StudentLoginModel());
        }

        public ActionResult Signout()
        {
            //Session.Clear();
            //FormsAuthentication.SignOut();
            return View("StudentLogin", new StudentLoginModel());
        }
    }
}