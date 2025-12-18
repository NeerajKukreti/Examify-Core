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
        var response = await _httpClient.GetAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models?key={_apiKey}");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error {response.StatusCode}: {errorContent}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> ExtractFromImageAsync(byte[] imageBytes, string mimeType = "image/png")
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
DO NOT REASON. DO NOT EXPLAIN. DO NOT THINK ABOUT THE CONTENT.

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

If something is ambiguous, return it literally. Never attempt to infer or guess anything.

----------------------------------------
EXCLUDE HEADER AND FOOTER
----------------------------------------
Ignore and DO NOT include:
- Page numbers
- Coaching institute names
- URLs, phone numbers
- Decorative footer/header text

Extract ONLY real MCQ questions and diagrams.

----------------------------------------
STEP 1 — OCR (EXACT TRANSCRIPTION)
----------------------------------------
Transcribe text EXACTLY as printed:
- Math equations in LaTeX: $x^2$, $H_2O$, $Fe^{3+}$
- Chemical formulas with subscripts/superscripts
- Arrows: →, ⇌, ↦
- NO simplification, NO interpretation

MULTI-LANGUAGE SEPARATOR RULE (MANDATORY):
- If a question or option contains text in more than one language,
  you MUST insert a line break tag <br/> between each language block.
- Preserve the original text exactly.
- Do NOT translate.
- Do NOT merge languages.
- Do NOT add extra text.
- Use ONLY <br/> (not <br>, not newline).

----------------------------------------
OPTION EXTRACTION RULE (IMPORTANT)
----------------------------------------
You MUST extract options EXACTLY as printed, except numbering must be removed.

Supported formats include:
(1) (2) (3) (4)
1. 2. 3. 4.
(A) (B) (C) (D)
a) b) c) d)
i) ii) iii) iv)
(अ) (ब) (स) (द)

RULES:
- REMOVE numbering labels such as (1), 1., (A), A), i), (अ) ONLY when they appear at the beginning of an option.
- Preserve the text after numbering exactly.
- Return options in the same vertical order.

FORCED SPLITTING RULE (CRITICAL):
- If multiple numbered options appear on the SAME printed line, you MUST split them into separate options using their numbering labels as boundaries.
- Splitting options by numbering labels IS allowed and mandatory.
- Never merge multiple options into one.
- Return the cleaned options (without numbering) in top-to-bottom order.

----------------------------------------
STEP 2 — DIAGRAM DETECTION (CRITICAL)
----------------------------------------
Detect ALL diagrams in the page. A ""diagram"" is ANY non-text visual element:
- Shapes, graphs, charts
- Circuit diagrams
- Chemical structures
- Geometric figures
- Image-based question figures
- 3×3 pattern matrices

DIAGRAM RULES:
- If a question contains multiple diagrams, return each separately.
- Never merge diagrams into a single bounding box.
- Never skip a small or secondary diagram.

BOUNDING BOX RULES:
1. Draw a TIGHT rectangle around ONLY the diagram.
2. EXCLUDE all text, labels, and numbers.
3. EXCLUDE unnecessary whitespace.
4. Include ONLY the graphical content.
5. Coordinates must be percentages (0–100):
   - x_percent
   - y_percent
   - width_percent
   - height_percent

----------------------------------------
STEP 3 — STRICT JSON OUTPUT
----------------------------------------
Return ONLY valid JSON in the structure below:

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
- NO markdown
- NO backticks
- NO explanations
- If no diagrams: ""diagrams"": []
- Output MUST be valid JSON only
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
                throw new Exception($"API Error {response.StatusCode}: {error}");
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
}
