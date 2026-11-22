using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Фиксируем HTTP-порт 5000 по умолчанию
builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Строка подключения читается по ключу ConnectionStrings:Default из user-secrets или переменных окружения
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULT")
        ?? "Host=localhost;Port=5432;Database=kontrorder;Username=postgres";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Сервис работает с AppDbContext (PostgreSQL), поэтому регистрируется как Scoped
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// При старте применяем миграции и при пустых таблицах заполняем начальные данные
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        DbSeeder.SeedIfEmpty(db);
    }
    catch (Exception ex)
    {
        logger.LogWarning(
            "Database migration/seed skipped or failed on startup; error={ErrorType}; message={Message}",
            ex.GetType().Name,
            ex.Message);
    }
}

// Безопасный перехват неожиданных ошибок (500): подробности пишутся в лог, клиенту уходит ProblemDetails без stack trace
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(
            "UnhandledRequest failed; path={Path}; error={ErrorType}",
            context.Request.Path.Value,
            ex.GetType().Name);

        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Сервер не смог обработать запрос",
                Detail = "Произошла внутренняя ошибка на сервере. Повторите попытку позже."
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
