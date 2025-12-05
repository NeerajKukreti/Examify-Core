namespace Examify.Services.OCR;

public class GeminiModelService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiModelService(IConfiguration configuration, HttpClient httpClient)
    {
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey");
        _httpClient = httpClient;
    }

    public async Task<string> ListAvailableModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models?key={_apiKey}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"API Error {response.StatusCode}: {errorContent}";
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}
