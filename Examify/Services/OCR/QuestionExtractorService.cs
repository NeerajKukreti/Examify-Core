using DataModel;
using System.Text;
using System.Text.Json;

namespace Examify.Services.OCR;

public interface IQuestionExtractorService
{
    Task<List<int>> SaveQuestionsAsync(List<QuestionModel> questions, int instituteId);
}

public class QuestionExtractorService : IQuestionExtractorService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public QuestionExtractorService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<int>> SaveQuestionsAsync(List<QuestionModel> questions, int instituteId)
    {
        var client = _httpClientFactory.CreateClient("ExamifyAPI");
        var json = JsonSerializer.Serialize(questions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"QuestionExtractor/SaveQuestions?instituteId={instituteId}", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<SaveQuestionsResponse>(result);
        return data?.QuestionIds ?? new List<int>();
    }

    private class SaveQuestionsResponse
    {
        public List<int> QuestionIds { get; set; } = new();
        public int Count { get; set; }
    }
}
