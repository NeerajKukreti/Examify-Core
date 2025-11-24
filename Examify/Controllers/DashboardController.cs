using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Examify.Extensions;

namespace Examify.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ActionResult Index()
        {
            var role = User.GetRole();
            var userId = User.GetUserId();
            var instituteId = User.GetInstituteId();
            
            ViewBag.UserId = userId;
            ViewBag.InstituteId = instituteId;
            ViewBag.ApiBaseUrl = _configuration["ExamifyAPI:BaseUrl"] ?? "https://localhost:7271/api/";

            return role?.ToLower() switch
            {
                "student" => View("StudentDashboard"),
                "institute" => View("InstituteDashboard"),
                _ => View("Index")
            };
        }
      
    }
}
