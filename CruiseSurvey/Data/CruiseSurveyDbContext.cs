using CruiseSurvey.Models;
using Microsoft.EntityFrameworkCore;

namespace CruiseSurvey.Data;

public class CruiseSurveyDbContext : DbContext
{
    public CruiseSurveyDbContext(DbContextOptions<CruiseSurveyDbContext> options)
        : base(options)
    {
    }

    public DbSet<SurveySubmissionEntity> SurveySubmissions => Set<SurveySubmissionEntity>();
    public DbSet<SurveyAnswerEntity> SurveyAnswers => Set<SurveyAnswerEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SurveySubmissionEntity>(entity =>
        {
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.CompletedAt);
            entity.HasIndex(e => e.CruiseShipName);
        });

        modelBuilder.Entity<SurveyAnswerEntity>(entity =>
        {
            entity.HasIndex(e => new { e.SurveySubmissionId, e.QuestionId }).IsUnique();
        });
    }
}
