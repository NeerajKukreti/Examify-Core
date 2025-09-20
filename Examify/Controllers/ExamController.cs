using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Examify.Common;
using System.Text.Json;
using DataModel;

public class ExamController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSettings _apiSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExamController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _apiSettings = apiSettings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult Selection()
    {
        return View("ExamList");
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = _httpClientFactory.CreateClient(_apiSettings.ClientName);
        var response = await client.GetAsync($"{_apiSettings.Endpoints.ExamById}/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var exam = JsonSerializer.Deserialize<ExamModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            ViewBag.ExamId = id;
            ViewBag.UserId = JwtHelper.GetUserIdFromSession(_httpContextAccessor);
            ViewBag.ApiBaseUrl = _apiSettings.BaseUrl + "Exam";
            ViewBag.StartExamUrl = _apiSettings.StartExamUrl;
            ViewBag.ExamResultUrl = _apiSettings.ExamResultUrl;
            return View(exam);
        }
        ViewBag.ExamId = id;
        ViewBag.UserId = JwtHelper.GetUserIdFromSession(_httpContextAccessor);
        ViewBag.ApiBaseUrl = _apiSettings.BaseUrl + "Exam";
        ViewBag.StartExamUrl = _apiSettings.StartExamUrl;
        ViewBag.ExamResultUrl = _apiSettings.ExamResultUrl;

        return NotFound();
    }
    
    public IActionResult StartExam(int examId)
    {
        ViewBag.ExamId = examId;
        ViewBag.UserId = JwtHelper.GetUserIdFromSession(_httpContextAccessor);
        ViewBag.ApiBaseUrl = _apiSettings.BaseUrl + "Exam";
        ViewBag.StartExamUrl = _apiSettings.StartExamUrl;
        ViewBag.ExamResultUrl = _apiSettings.ExamResultUrl;
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