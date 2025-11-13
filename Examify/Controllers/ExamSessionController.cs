using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Examify.Common;
using Examify.Extensions;
using System.Text.Json;
using DataModel;
using DataModel.Common;
using Examify.Common.constants;

[Authorize(Roles = "Student")]
public class ExamSessionController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSettings _apiSettings;

    public ExamSessionController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _apiSettings = apiSettings.Value;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult UserExam()
    {
        ViewBag.UserId = User.GetUserId();
        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = _httpClientFactory.CreateClient(ENDPOINTS.ClientName);
        var response = await client.GetAsync($"{ENDPOINTS.ExamById}/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<ExamModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var exam = apiResponse?.Data;   
            ViewBag.ExamId = id;
            ViewBag.UserId = User.GetUserId();
            ViewBag.ApiBaseUrl = ENDPOINTS.BaseUrl + "Exam";
            ViewBag.StartExamUrl = ENDPOINTS.StartExamUrl;
            ViewBag.ExamResultUrl = ENDPOINTS.ExamResultUrl;
            return View(exam);
        }
        ViewBag.ExamId = id;
        ViewBag.UserId = User.GetUserId();
        ViewBag.ApiBaseUrl = ENDPOINTS.BaseUrl + "Exam";
        ViewBag.StartExamUrl = ENDPOINTS.StartExamUrl;
        ViewBag.ExamResultUrl = ENDPOINTS.ExamResultUrl;

        return NotFound();
    }
    
    public IActionResult StartExam(int examId)
    {
        ViewBag.ExamId = examId;
        ViewBag.UserId = User.GetUserId();
        ViewBag.ApiBaseUrl = ENDPOINTS.BaseUrl + "Exam";
        ViewBag.StartExamUrl = ENDPOINTS.StartExamUrl;
        ViewBag.ExamResultUrl = ENDPOINTS.ExamResultUrl;
        return View();
    }
    
    public IActionResult ExamResult(int sessionId)
    {
        ViewBag.SessionId = sessionId;
        return View();
    }
    
    public IActionResult Test()
    {
        return View();
    }
    
    
}