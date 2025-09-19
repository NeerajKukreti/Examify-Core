using Microsoft.AspNetCore.Mvc;
using OnlineExam.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; 

namespace OnlineExam.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            
            return View();
        }
    }
}
