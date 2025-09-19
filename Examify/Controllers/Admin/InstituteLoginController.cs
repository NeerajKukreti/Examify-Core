using DataModel;
using Microsoft.AspNetCore.Mvc;
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
    public class InstituteLoginController : Controller
    {
        private readonly IInstituteLoginService _instituteLoginService;
        public InstituteLoginController(IInstituteLoginService instituteLoginService)
        {
            _instituteLoginService = instituteLoginService;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> index(InstituteLoginModel InstituteLogin)
        {
            InstituteLogin= await _instituteLoginService.InstituteLoginDetails(InstituteLogin);
            if (InstituteLogin.StatusCode > 0)
            {
                //FormsAuthentication.SetAuthCookie(InstituteLogin.PrimaryContact, false);
                
                //Session.Add("Institute", InstituteLogin);
                //Session.Add("Role", "Institute");

                return RedirectToAction("index", "dashboard");
                
            }
            else
            {
                return View("InstituteLogin", InstituteLogin);
            }
            

        }

        // GET: Admin
        public ActionResult Index()
        {
            return View("InstituteLogin", new InstituteLoginModel());
        }

        public ActionResult Signout()
        {
            //Session.Clear();
            //FormsAuthentication.SignOut();
            return View("InstituteLogin", new InstituteLoginModel());
        }
    }
}