using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using DataModel;
using Examify.Services;

using Microsoft.AspNetCore.Authorization;

namespace Examify.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IExamifyApiClient _apiClient;

        public HomeController(IExamifyApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var publicExams = new List<ExamModel>();
            try
            {
                var response = await _apiClient.GetPublicCatalogAsync();
                if (response.Success && response.Data != null)
                {
                    publicExams = response.Data;
                }
            }
            catch
            {
                // Fallback gracefully if API is unreachable
            }

            ViewBag.PublicExams = publicExams;
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
