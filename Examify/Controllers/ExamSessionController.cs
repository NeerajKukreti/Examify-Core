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

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ExamifyAPI");
            var isAuth = User?.Identity?.IsAuthenticated == true;
            var endpoint = isAuth ? $"Exam/Session/{id}" : $"Exam/{id}";
            var response = await client.GetAsync(endpoint);

            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ExamModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var exam = apiResponse?.Data;
                ViewBag.ExamId = id;
                ViewBag.UserId = isAuth ? User.GetUserId() : "0";
                ViewBag.IsAuthenticated = isAuth;
                ViewBag.ApiBaseUrl = client.BaseAddress + "Exam";
                ViewBag.StartExamUrl = "/ExamSession/StartExam";
                ViewBag.ExamResultUrl = "/ExamSession/ExamResult";
                return View(exam);
            }

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    ViewBag.ErrorMessage = errorResponse?.Message ?? "An error occurred";
                }
                catch
                {
                    ViewBag.ErrorMessage = json;
                }
            }
            else
            {
                ViewBag.ErrorMessage = $"Failed to retrieve exam details (HTTP {(int)response.StatusCode})";
            }
            return View("Error");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            return Content(ex.Message);
        }
    }


    public IActionResult StartExam(int examId)
    {
        ViewBag.ExamId = examId;
        ViewBag.UserId = User.GetUserId();
        var client = _httpClientFactory.CreateClient("ExamifyAPI");
        ViewBag.ApiBaseUrl = client.BaseAddress + "Exam";
        ViewBag.StartExamUrl = "/ExamSession/StartExam";
        ViewBag.ExamResultUrl = "/ExamSession/ExamResult";
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