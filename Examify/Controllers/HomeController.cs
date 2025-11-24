using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace Examify.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            
            ViewBag.ErrorMessage = exceptionFeature?.Error?.Message ?? "An unexpected error occurred";
            ViewBag.ErrorDetails = exceptionFeature?.Error?.ToString();
            ViewBag.Path = exceptionFeature?.Path;
            
            return View();
        }
    }
}
