using System.Text.Json;
using System.Text.RegularExpressions;

namespace Examify.Services.OCR;

public class GeminiJsonParserService
{
    public (List<OcrQuestion> Questions, Dictionary<int, byte[]> DiagramImages) ParseJsonResponse(string jsonText)
    {
        var questions = new List<OcrQuestion>();

        if (jsonText.StartsWith("```"))
        {
            var lines = jsonText.Split('\n');
            jsonText = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
        }

        jsonText = jsonText.Trim();

        using var doc = JsonDocument.Parse(jsonText);
        var root = doc.RootElement;

        var questionsToProcess = root.TryGetProperty("questions", out var questionsArray)
            ? questionsArray.EnumerateArray()
            : (root.ValueKind == JsonValueKind.Array ? root.EnumerateArray() : new[] { root }.AsEnumerable());

        foreach (var questionObj in questionsToProcess)
        {
            var questionText = questionObj.GetProperty("question_text").GetString() ?? "";
            var options = questionObj.GetProperty("options");
            
            int questionNumber = 0;
            if (questionObj.TryGetProperty("question_number", out var qNumProp))
            {
                questionNumber = qNumProp.GetInt32();
            }
            else
            {
                var qNumMatch = Regex.Match(questionText, @"^(\d+)\.");
                questionNumber = qNumMatch.Success ? int.Parse(qNumMatch.Groups[1].Value) : 0;
            }

            var question = new OcrQuestion
            {
                QuestionNumber = questionNumber,
                QuestionText = questionText,
                Options = new List<string>()
            };

            foreach (var opt in options.EnumerateArray())
            {
                question.Options.Add(opt.GetString() ?? "");
            }

            if (questionObj.TryGetProperty("diagrams", out var diagramsArray))
            {
                question.Diagrams = new List<OcrDiagram>();
                foreach (var diagramObj in diagramsArray.EnumerateArray())
                {
                    question.Diagrams.Add(new OcrDiagram
                    {
                        QuestionNumber = diagramObj.TryGetProperty("question_number", out var dqn) ? dqn.GetInt32() : questionNumber,
                        Description = diagramObj.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                        XPercent = diagramObj.TryGetProperty("x_percent", out var xp) ? xp.GetDouble() : 0,
                        YPercent = diagramObj.TryGetProperty("y_percent", out var yp) ? yp.GetDouble() : 0,
                        WidthPercent = diagramObj.TryGetProperty("width_percent", out var wp) ? wp.GetDouble() : 0,
                        HeightPercent = diagramObj.TryGetProperty("height_percent", out var hp) ? hp.GetDouble() : 0
                    });
                }
            }

            questions.Add(question);
        }

        return (questions, new Dictionary<int, byte[]>());
    }
}

public class OcrQuestion
{
    public int QuestionNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public List<OcrDiagram>? Diagrams { get; set; }
}

public class OcrDiagram
{
    public int QuestionNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public double XPercent { get; set; }
    public double YPercent { get; set; }
    public double WidthPercent { get; set; }
    public double HeightPercent { get; set; }
}
