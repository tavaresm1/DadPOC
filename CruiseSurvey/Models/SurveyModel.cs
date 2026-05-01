using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CruiseSurvey.Models;

public class PersonalInfo
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your age range.")]
    public string AgeRange { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cruise ship name is required.")]
    public string CruiseShipName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departure date is required.")]
    public DateTime? DepartureDate { get; set; }

    [Required(ErrorMessage = "Please enter the number of nights.")]
    [Range(1, 90, ErrorMessage = "Please enter a valid number of nights (1-90).")]
    public int? NumberOfNights { get; set; }
}

public class SurveyResponse
{
    public PersonalInfo PersonalInfo { get; set; } = new();
    public Dictionary<int, int> Ratings { get; set; } = new();
    public Dictionary<int, string> Comments { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}

public static class SurveyQuestions
{
    public static readonly List<SurveyQuestion> Questions = new()
    {
        new(1, "Overall Experience", "How would you rate your overall cruise experience?"),
        new(2, "Cabin & Accommodations", "How satisfied were you with your cabin and accommodations?"),
        new(3, "Dining Experience", "How would you rate the quality of the food and dining options?"),
        new(4, "Entertainment & Activities", "How would you rate the onboard entertainment and activities?"),
        new(5, "Staff & Service", "How would you rate the friendliness and helpfulness of the crew?"),
        new(6, "Cleanliness", "How would you rate the cleanliness of the ship and facilities?"),
        new(7, "Shore Excursions", "How satisfied were you with the ports of call and shore excursions?"),
        new(8, "Value for Money", "How would you rate the overall value for the price you paid?"),
        new(9, "Embarkation & Debarkation", "How smooth was the boarding and departure process?"),
        new(10, "Likelihood to Recommend", "How likely are you to recommend this cruise to a friend or family member?"),
    };
}

public record SurveyQuestion(int Id, string Category, string QuestionText);

[Table("SurveySubmissions")]
public class SurveySubmissionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string AgeRange { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string CruiseShipName { get; set; } = string.Empty;

    public DateTime DepartureDate { get; set; }

    public int NumberOfNights { get; set; }

    public DateTime CompletedAt { get; set; }

    [Column(TypeName = "decimal(3,1)")]
    public decimal AverageRating { get; set; }

    public List<SurveyAnswerEntity> Answers { get; set; } = new();
}

[Table("SurveyAnswers")]
public class SurveyAnswerEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int SurveySubmissionId { get; set; }

    public int QuestionId { get; set; }

    [Required, StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    public int Rating { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }

    [ForeignKey(nameof(SurveySubmissionId))]
    public SurveySubmissionEntity Submission { get; set; } = null!;
}
