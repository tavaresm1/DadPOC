using CruiseSurvey.Models;

namespace CruiseSurvey.Services;

public class SurveyService
{
    private readonly string _outputDirectory;

    public SurveyService(IWebHostEnvironment env)
    {
        _outputDirectory = Path.Combine(env.ContentRootPath, "SurveyResults");
        Directory.CreateDirectory(_outputDirectory);
    }

    public async Task<string> SaveSurveyResultsAsync(SurveyResponse response)
    {
        var fileName = $"survey_{response.PersonalInfo.LastName}_{response.PersonalInfo.FirstName}_{response.CompletedAt:yyyyMMdd_HHmmss}.txt";
        var filePath = Path.Combine(_outputDirectory, fileName);

        var content = BuildSummary(response);
        await File.WriteAllTextAsync(filePath, content);
        return filePath;
    }

    private static string BuildSummary(SurveyResponse response)
    {
        var info = response.PersonalInfo;
        var ratings = response.Ratings;
        var comments = response.Comments;
        var questions = SurveyQuestions.Questions;

        var avg = ratings.Count > 0 ? ratings.Values.Average() : 0;

        var lines = new List<string>
        {
            "╔══════════════════════════════════════════════════════════╗",
            "║           CRUISE EXPERIENCE SURVEY RESULTS              ║",
            "╚══════════════════════════════════════════════════════════╝",
            "",
            "PERSONAL INFORMATION",
            new string('─', 55),
            $"  Name:            {info.FirstName} {info.LastName}",
            $"  Email:           {info.Email}",
            $"  Age Range:       {info.AgeRange}",
            $"  Cruise Ship:     {info.CruiseShipName}",
            $"  Departure Date:  {info.DepartureDate:MMMM dd, yyyy}",
            $"  Number of Nights:{info.NumberOfNights}",
            $"  Survey Completed:{response.CompletedAt:MMMM dd, yyyy hh:mm tt}",
            "",
            "SURVEY RESPONSES",
            new string('─', 55),
        };

        foreach (var q in questions)
        {
            var rating = ratings.GetValueOrDefault(q.Id, 0);
            var stars = new string('★', rating) + new string('☆', 5 - rating);
            lines.Add($"  Q{q.Id}. {q.Category}");
            lines.Add($"      \"{q.QuestionText}\"");
            lines.Add($"      Rating: {stars} ({rating}/5)");

            if (comments.TryGetValue(q.Id, out var comment) && !string.IsNullOrWhiteSpace(comment))
            {
                lines.Add($"      Comment: {comment}");
            }

            lines.Add("");
        }

        lines.Add(new string('─', 55));
        lines.Add($"  OVERALL AVERAGE SCORE: {avg:F1} / 5.0");
        lines.Add("");

        var sentiment = avg switch
        {
            >= 4.5 => "Exceptional - Guest had an outstanding experience!",
            >= 3.5 => "Positive - Guest was generally satisfied with their cruise.",
            >= 2.5 => "Mixed - Some areas met expectations, others need improvement.",
            >= 1.5 => "Below Expectations - Guest was largely dissatisfied.",
            _ => "Poor - Significant issues reported across the board.",
        };
        lines.Add($"  SENTIMENT: {sentiment}");
        lines.Add("");
        lines.Add(new string('═', 55));
        lines.Add("  End of Survey Report");
        lines.Add(new string('═', 55));

        return string.Join(Environment.NewLine, lines);
    }
}
