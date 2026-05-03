using Newproject.Components;
using Newproject.Data;
using Newproject.Services;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(DockerSqlServerService.ConnectionString));

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
await DockerSqlServerService.EnsureRunningAsync(logger);

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    logger.LogInformation("Applying database migrations...");
    await db.Database.EnsureCreatedAsync();
    logger.LogInformation("Database schema is ready.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
