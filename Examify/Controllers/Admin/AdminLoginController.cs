using DataModel;
using Examify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnlineExam.Common;
using OnlineExam.Helpers;
using OnlineExam.Services;
using System.Diagnostics;

namespace OnlineExam.Controllers
{
    public class AdminLoginController : Controller
    {
        private readonly AppSettings _settings;
        private readonly IFileService _fileService;
        private readonly IAdminLoginService _adminLoginService;
        public AdminLoginController(IFileService fileService, 
            IOptions<AppSettings> settings, 
            IAdminLoginService adminLoginService)
        {
            _fileService = fileService;
            _settings = settings.Value;
            _adminLoginService = adminLoginService;
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> index(AdminLoginModel AdminLogin)
        {
            AdminLogin= await _adminLoginService.AdminLoginDetails(AdminLogin);
            if (AdminLogin.StatusCode > 0)
            {
                //FormsAuthentication.SetAuthCookie(AdminLogin.MobileNumber, false);
                //Session.Add("AdminMobile", AdminLogin.MobileNumber);
                //Session.Add("AdminName", AdminLogin.AdminName);
                //Session.Add("Role", "Admin");

                return RedirectToAction("index", "dashboard");
            }
            else
            {
                return View("AdminLogin", AdminLogin);
            }
            

        }

        // GET: Admin
        public ActionResult Index()
        {
            return View("AdminLogin", new AdminLoginModel());
        }

        public ActionResult Signout()
        {
            //Session.Clear();
            //FormsAuthentication.SignOut();
            return View("AdminLogin", new AdminLoginModel());
        }
    }
}