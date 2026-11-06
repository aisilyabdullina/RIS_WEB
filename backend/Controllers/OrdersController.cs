using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/orders")]
[Route("api/tickets")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех заказов (включая пустой массив [] со статусом 200).
    /// </summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<OrderResponse>> GetAll()
    {
        return Ok(_orderService.GetAll());
    }

    /// <summary>
    /// Получить один заказ по id (200 или 404).
    /// </summary>
    [HttpGet("{id:int}")]
    public ActionResult<OrderResponse> GetById(int id)
    {
        var item = _orderService.GetById(id);
        if (item is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Запись не найдена",
                Detail = $"Заказ с id={id} не существует."
            });
        }

        return Ok(item);
    }

    /// <summary>
    /// Создать новый заказ (201 Created или 400 Bad Request).
    /// </summary>
    [HttpPost]
    public ActionResult<OrderResponse> Create([FromBody] CreateOrderRequest? request)
    {
        var modelId = request?.ResolvedModelId ?? 0;

        try
        {
            var result = _orderService.Create(request);
            if (!result.IsSuccess)
            {
                return ValidationProblem(new ValidationProblemDetails(
                    result.ValidationErrors ?? new Dictionary<string, string[]>())
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Ошибка проверки полей",
                    Detail = result.ErrorMessage
                });
            }

            // Для проверки п. 5.3 методички (раскомментировать временно для скриншота лога 500, затем снова закомментировать):
            // throw new InvalidOperationException("Simulated unexpected failure after validation");

            var created = result.Data!;
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            // Пункт 5.3: одна понятная строка в логе сервера и безопасный ProblemDetails клиенту без stack trace
            _logger.LogError(
                "CreateOrder failed; modelId={ModelId}; error={ErrorType}",
                modelId,
                ex.GetType().Name);

            return Problem(
                title: "Сервер не смог обработать запрос",
                detail: "Неожиданный сбой при создании записи.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Отдельно назначить исполнителя заказа (200, 400, 404 или 409 для завершённого заказа).
    /// </summary>
    [HttpPatch("{id:int}/assignee")]
    public ActionResult<OrderResponse> AssignAssignee(int id, [FromBody] AssignAssigneeRequest? request)
    {
        var result = _orderService.AssignUser(id, request);
        return ToActionResult(result);
    }

    /// <summary>
    /// Отдельно изменить статус заказа (200, 400, 404 или 409 при недопустимом переходе / отсутствии исполнителя для InProgress).
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public ActionResult<OrderResponse> ChangeStatus(int id, [FromBody] ChangeStatusRequest? request)
    {
        var result = _orderService.ChangeStatus(id, request);
        return ToActionResult(result);
    }

    private ActionResult<OrderResponse> ToActionResult(ServiceResult<OrderResponse> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return result.ErrorType switch
        {
            OperationErrorType.Validation => ValidationProblem(new ValidationProblemDetails(
                result.ValidationErrors ?? new Dictionary<string, string[]>())
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Ошибка проверки полей",
                Detail = result.ErrorMessage
            }),
            OperationErrorType.NotFound => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Запись не найдена",
                Detail = result.ErrorMessage
            }),
            OperationErrorType.Conflict => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Конфликт состояния процесса",
                Detail = result.ErrorMessage
            }),
            _ => Problem(
                title: "Сервер не смог обработать запрос",
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
