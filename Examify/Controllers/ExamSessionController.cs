using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Examify.Common;
using System.Text.Json;
using DataModel;
using Examify.Common.constants;

[Authorize(Roles = "Student")]
public class ExamSessionController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSettings _apiSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExamSessionController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _apiSettings = apiSettings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult UserExam()
    {
        ViewBag.UserId = JwtHelper.GetUserIdFromSession(_httpContextAccessor);
        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = _httpClientFactory.CreateClient(ENDPOINTS.ClientName);
        var response = await client.GetAsync($"{ENDPOINTS.ExamById}/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<JsonElement>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var exam = JsonSerializer.Deserialize<ExamModel>(apiResponse.GetProperty("Data").GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });   
            ViewBag.ExamId = id;
            ViewBag.UserId = JwtHelper.GetUserIdFromSession(_httpContextAccessor);
            ViewBag.ApiBaseUrl = ENDPOINTS.BaseUrl + "Exam";
            ViewBag.StartExamUrl = ENDPOINTS.StartExamUrl;
            ViewBag.ExamResultUrl = ENDPOINTS.ExamResultUrl;
            return View(exam);
        }
        ViewBag.ExamId = id;
        ViewBag.UserId = JwtHelper.GetUserIdFromSession(_httpContextAccessor);
        ViewBag.ApiBaseUrl = ENDPOINTS.BaseUrl + "Exam";
        ViewBag.StartExamUrl = ENDPOINTS.StartExamUrl;
        ViewBag.ExamResultUrl = ENDPOINTS.ExamResultUrl;

        return NotFound();
    }
    
    public IActionResult StartExam(int examId)
    {
        ViewBag.ExamId = examId;
        ViewBag.UserId = JwtHelper.GetUserIdFromSession(_httpContextAccessor);
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