using CruiseSurvey.Data;
using CruiseSurvey.Models;
using Microsoft.EntityFrameworkCore;

namespace CruiseSurvey.Services;

public class SurveyService
{
    private readonly IDbContextFactory<CruiseSurveyDbContext> _dbFactory;
    private readonly string _outputDirectory;

    public SurveyService(IDbContextFactory<CruiseSurveyDbContext> dbFactory, IWebHostEnvironment env)
    {
        _dbFactory = dbFactory;
        _outputDirectory = Path.Combine(env.ContentRootPath, "SurveyResults");
        Directory.CreateDirectory(_outputDirectory);
    }

    public async Task<string> SaveSurveyResultsAsync(SurveyResponse response)
    {
        await SaveToDatabaseAsync(response);

        var fileName = $"survey_{response.PersonalInfo.LastName}_{response.PersonalInfo.FirstName}_{response.CompletedAt:yyyyMMdd_HHmmss}.txt";
        var filePath = Path.Combine(_outputDirectory, fileName);
        var content = BuildSummary(response);
        await File.WriteAllTextAsync(filePath, content);

        return filePath;
    }

    private async Task SaveToDatabaseAsync(SurveyResponse response)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var info = response.PersonalInfo;
        var avg = response.Ratings.Count > 0 ? (decimal)response.Ratings.Values.Average() : 0m;

        var submission = new SurveySubmissionEntity
        {
            FirstName = info.FirstName,
            LastName = info.LastName,
            Email = info.Email,
            AgeRange = info.AgeRange,
            CruiseShipName = info.CruiseShipName,
            DepartureDate = info.DepartureDate ?? DateTime.MinValue,
            NumberOfNights = info.NumberOfNights ?? 0,
            CompletedAt = response.CompletedAt,
            AverageRating = Math.Round(avg, 1),
        };

        foreach (var question in SurveyQuestions.Questions)
        {
            var rating = response.Ratings.GetValueOrDefault(question.Id, 0);
            response.Comments.TryGetValue(question.Id, out var comment);

            submission.Answers.Add(new SurveyAnswerEntity
            {
                QuestionId = question.Id,
                Category = question.Category,
                QuestionText = question.QuestionText,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment,
            });
        }

        db.SurveySubmissions.Add(submission);
        await db.SaveChangesAsync();
    }

    public async Task<List<SurveySubmissionEntity>> GetAllSurveysAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.SurveySubmissions
            .Include(s => s.Answers)
            .OrderByDescending(s => s.CompletedAt)
            .ToListAsync();
    }

    public async Task<SurveySubmissionEntity?> GetSurveyByIdAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.SurveySubmissions
            .Include(s => s.Answers.OrderBy(a => a.QuestionId))
            .FirstOrDefaultAsync(s => s.Id == id);
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
            "======================================================",
            "           CRUISE EXPERIENCE SURVEY RESULTS            ",
            "======================================================",
            "",
            "PERSONAL INFORMATION",
            new string('-', 55),
            $"  Name:             {info.FirstName} {info.LastName}",
            $"  Email:            {info.Email}",
            $"  Age Range:        {info.AgeRange}",
            $"  Cruise Ship:      {info.CruiseShipName}",
            $"  Departure Date:   {info.DepartureDate:MMMM dd, yyyy}",
            $"  Number of Nights: {info.NumberOfNights}",
            $"  Survey Completed: {response.CompletedAt:MMMM dd, yyyy hh:mm tt}",
            "",
            "SURVEY RESPONSES",
            new string('-', 55),
        };

        foreach (var q in questions)
        {
            var rating = ratings.GetValueOrDefault(q.Id, 0);
            lines.Add($"  Q{q.Id}. {q.Category}");
            lines.Add($"      \"{q.QuestionText}\"");
            lines.Add($"      Rating: {rating}/5");

            if (comments.TryGetValue(q.Id, out var comment) && !string.IsNullOrWhiteSpace(comment))
            {
                lines.Add($"      Comment: {comment}");
            }

            lines.Add("");
        }

        lines.Add(new string('-', 55));
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
        lines.Add(new string('=', 55));
        lines.Add("  End of Survey Report");
        lines.Add(new string('=', 55));

        return string.Join(Environment.NewLine, lines);
    }
}
