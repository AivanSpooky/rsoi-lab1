using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonService.Data;
using PersonService.Dto;
using PersonService.Infrastructure;
using PersonService.Services;

var builder = WebApplication.CreateBuilder(args);

var port = builder.Configuration["PORT"];
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddDbContext<PersonDbContext>(options =>
    options.UseNpgsql(
        DatabaseConnection.Resolve(builder.Configuration),
        npgsql => npgsql.EnableRetryOnFailure()));

builder.Services.AddScoped<IPersonService, PersonServiceImpl>();

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(ValidationErrorResponse.FromModelState(context.ModelState)));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<PersonNotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await MigrateDatabaseAsync(app);

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/manage/health", () => Results.Ok(new { status = "UP" }));

app.Run();

static async Task MigrateDatabaseAsync(WebApplication app)
{
    const int maxAttempts = 12;
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PersonDbContext>();

    for (var attempt = 1; ; attempt++)
    {
        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database schema is up to date");
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            logger.LogWarning(exception, "Database is not ready (attempt {Attempt}/{MaxAttempts}), retrying",
                attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

public partial class Program;
