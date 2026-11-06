using backend.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Фиксируем HTTP-порт 5000 по умолчанию
builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация сервиса хранения в памяти через DI (Singleton, чтобы список сохранялся между запросами в рамках запуска)
builder.Services.AddSingleton<IOrderService, OrderService>();

var app = builder.Build();

// Безопасный перехват неожиданных ошибок (500): лог без секретов на сервере, клиенту только безопасный ProblemDetails без stack trace
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
