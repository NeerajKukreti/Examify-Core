using System.Text;
using System.Text.Json;

namespace Examify.Services.OCR;

public class GeminiOcrService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiOcrService(IConfiguration configuration, HttpClient httpClient)
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

    public async Task<string> ExtractFromImageAsync(byte[] imageBytes, string mimeType = "image/png")
    {
        try
        {
            var base64Image = Convert.ToBase64String(imageBytes);

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { inline_data = new { mime_type = mimeType, data = base64Image } },
                            new
                            {
                                text = @"
You are a precise OCR + diagram detection engine. Extract MCQ questions and locate diagrams with EXACT bounding boxes.

----------------------------------------
CRITICAL RULE TO PREVENT FAILURE
----------------------------------------
DO NOT REASON.
DO NOT EXPLAIN.
DO NOT THINK ABOUT THE CONTENT.

You MUST NOT:
- Identify patterns
- Solve the question
- Interpret diagrams
- Deduce missing shapes
- Compare figures
- Perform logical reasoning

You MUST ONLY:
- Transcribe text exactly
- Detect diagrams
- Return coordinates + descriptions
- Output valid JSON

If something is ambiguous, return it literally.
Never attempt to infer or guess anything.

=== STEP 1: OCR (EXACT TRANSCRIPTION) ===
Transcribe text EXACTLY as printed:
- Math equations in LaTeX: $x^2$, $H_2O$, $Fe^{3+}$
- Chemical formulas with subscripts/superscripts
- Arrows: →, ⇌, ↦
- NO simplification, NO interpretation

OPTIONS EXTRACTION RULE (IMPORTANT):
You MUST extract options exactly as printed.

Supported formats include but are not limited to:
(1) (2) (3) (4)
1. 2. 3. 4.
A B C D
(A) (B) (C) (D)
a) b) c) d)
i) ii) iii) iv)
(अ) (ब) (स) (द)

RULES:
- Preserve the EXACT text and numbering format.
- If options appear on separate lines without labels, treat EACH line as one option.
- NEVER infer missing options.
- NEVER renumber options.
- RETURN options in the order they appear vertically.

=== STEP 2: DIAGRAM DETECTION (CRITICAL) ===
Scan the ENTIRE image and detect ALL diagrams present.
If a single question contains more than one diagram, you MUST return each diagram as a separate entry in the ""diagrams"" array.
Never merge multiple diagrams into a single bounding box. 
Never skip a secondary or smaller diagram.
Even if diagrams appear side-by-side or stacked, detect each one individually.
Return all diagrams exactly as they appear, each with its own bounding box and description.

A DIAGRAM is any NON-TEXT visual element:
- Shapes (circles, triangles, rectangles)
- Graphs, charts, plots
- Chemical structures
- Geometric figures
- Circuit diagrams
- Figure matrices (3×3 grids of shapes)

BOUNDING BOX RULES:
1. Draw a TIGHT rectangle around ONLY the diagram
2. EXCLUDE text, labels, question numbers
3. EXCLUDE whitespace
4. Include ONLY the graphic
5. Measurement from edges as percentages (0–100)

=== STEP 3: OUTPUT (STRICT JSON) ===
{
  ""questions"": [
    {
      ""question_number"": 1,
      ""question_text"": ""..."",
      ""options"": [""..."", ""..."", ""..."", ""...""],
      ""diagrams"": [
        {
          ""question_number"": 1,
          ""description"": ""..."",
          ""x_percent"": 30.0,
          ""y_percent"": 25.0,
          ""width_percent"": 40.0,
          ""height_percent"": 18.75
        }
      ]
    }
  ]
}

RULES:
- NO markdown, NO backticks, NO explanations
- If no diagram: ""diagrams"": []
- VALID JSON ONLY

"
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.0,
                    maxOutputTokens = 16384
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={_apiKey}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"API Error {response.StatusCode}: {error}";
            }

            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var candidates = doc.RootElement.GetProperty("candidates");
            var contentNode = candidates[0].GetProperty("content");
            var parts = contentNode.GetProperty("parts");

            var finalResponse = new StringBuilder();

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var txt))
                    finalResponse.Append(txt.GetString());
            }

            var jsonText = finalResponse.ToString().Trim();

            if (jsonText.StartsWith("```json"))
                jsonText = jsonText.Substring(7);
            else if (jsonText.StartsWith("```"))
                jsonText = jsonText.Substring(3);

            if (jsonText.EndsWith("```"))
                jsonText = jsonText.Substring(0, jsonText.Length - 3);

            jsonText = jsonText.Trim();

            var jsonFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "gemini-responses");
            Directory.CreateDirectory(jsonFolder);
            var jsonFileName = $"response_{DateTime.Now:yyyyMMddHHmmss}.json";
            var jsonFilePath = Path.Combine(jsonFolder, jsonFileName);
            await File.WriteAllTextAsync(jsonFilePath, jsonText);

            return jsonText;
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}