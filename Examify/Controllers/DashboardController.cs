using Microsoft.AspNetCore.Mvc;
using Examify.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web; 

namespace Examify.Controllers
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
