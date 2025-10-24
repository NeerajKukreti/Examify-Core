using DataModel;
using Examify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Examify.Common;
using Examify.Helpers;
using Examify.Services;
using System.Diagnostics;

namespace Examify.Controllers
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
                // Store both access and refresh tokens if available
                // Note: Update AdminLoginService to return tokens
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