using Microsoft.AspNetCore.Mvc;
using Examify.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.TryParse(HttpContext.Session.GetString("UserId"), out var uid) ? uid : 1; 
            
            ViewBag.UserId = userId;
            ViewBag.InstituteId = userId;
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7271/api";

            return role?.ToLower() switch
            {
                "student" => View("StudentDashboard"),
                "institute" => View("InstituteDashboard"),
                _ => View("Index")
            };
        }
      
    }
}
