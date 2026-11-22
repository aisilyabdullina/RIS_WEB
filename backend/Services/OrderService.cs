using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OrderService : IOrderService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.Ordinal)
    {
        "New",
        "InProgress",
        "Closed",
        "Cancelled"
    };

    // Допустимые переходы статусов. Из Closed и Cancelled переходов нет.
    private static readonly Dictionary<string, string[]> AllowedTransitions = new(StringComparer.Ordinal)
    {
        ["New"] = new[] { "InProgress", "Cancelled" },
        ["InProgress"] = new[] { "Closed", "Cancelled" },
        ["Closed"] = Array.Empty<string>(),
        ["Cancelled"] = Array.Empty<string>()
    };

    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    public string GetApplicationName() => "КонтрОрдер (АО «НИЦ «Инкомсистем»)";

    public string GetLabStageNote() => "ЛР10: EF Core — сервер читает и пишет в базу PostgreSQL";

    public IReadOnlyList<OrderResponse> GetAll()
    {
        return _db.Orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .ToList()
            .Select(MapToResponse)
            .ToList();
    }

    public OrderResponse? GetById(int id)
    {
        var item = _db.Orders
            .AsNoTracking()
            .FirstOrDefault(o => o.Id == id);

        return item is null ? null : MapToResponse(item);
    }

    public ServiceResult<OrderResponse> Create(CreateOrderRequest? request)
    {
        if (request is null)
        {
            return ServiceResult<OrderResponse>.Validation("Тело запроса не может быть пустым.");
        }

        var cleanTitle = (request.Title ?? string.Empty).Trim();
        var cleanDescription = (request.Description ?? string.Empty).Trim();
        var modelId = request.ModelId ?? 0;
        var createdByUserId = request.CreatedByUserId is > 0 ? request.CreatedByUserId.Value : 1;

        var errors = new Dictionary<string, string[]>();

        if (cleanTitle.Length < 5 || cleanTitle.Length > 80)
        {
            errors["title"] = new[] { "Поле title должно содержать от 5 до 80 символов." };
        }

        if (modelId <= 0)
        {
            errors["modelId"] = new[] { "Поле modelId должно быть целым числом больше нуля." };
        }
        else if (!_db.ControllerModels.Any(m => m.Id == modelId))
        {
            errors["modelId"] = new[] { $"Модель контроллера с id={modelId} отсутствует в справочнике." };
        }

        if (cleanDescription.Length < 10 || cleanDescription.Length > 500)
        {
            errors["description"] = new[] { "Поле description должно содержать от 10 до 500 символов." };
        }

        if (!_db.Users.Any(u => u.Id == createdByUserId))
        {
            errors["createdByUserId"] = new[] { $"Пользователь-создатель с id={createdByUserId} не найден." };
        }

        if (errors.Count > 0)
        {
            return ServiceResult<OrderResponse>.Validation("Проверьте поля запроса.", errors);
        }

        var created = new Order
        {
            // Временный номер: постоянный формируется ниже по идентификатору, который выдаёт база
            Number = $"TMP-{Guid.NewGuid():N}",
            Title = cleanTitle,
            ModelId = modelId,
            Description = cleanDescription,
            Status = "New",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = createdByUserId,
            AssigneeUserId = null
        };

        _db.Orders.Add(created);
        _db.SaveChanges();

        created.Number = $"ЗК-2026-{created.Id:D4}";
        _db.SaveChanges();

        return ServiceResult<OrderResponse>.Success(MapToResponse(created));
    }

    public ServiceResult<OrderResponse> AssignUser(int id, AssignAssigneeRequest? request)
    {
        if (request is null || request.AssigneeUserId <= 0)
        {
            return ServiceResult<OrderResponse>.Validation(
                "Поле assigneeUserId должно быть целым числом больше нуля.",
                new Dictionary<string, string[]>
                {
                    ["assigneeUserId"] = new[] { "Укажите корректный идентификатор исполнителя (> 0)." }
                });
        }

        var item = _db.Orders.FirstOrDefault(o => o.Id == id);
        if (item is null)
        {
            return ServiceResult<OrderResponse>.NotFound($"Заказ с id={id} не найден.");
        }

        var assignee = _db.Users.FirstOrDefault(u => u.Id == request.AssigneeUserId);
        if (assignee is null)
        {
            return ServiceResult<OrderResponse>.Validation(
                $"Пользователь с id={request.AssigneeUserId} не существует.",
                new Dictionary<string, string[]>
                {
                    ["assigneeUserId"] = new[] { $"Пользователь с id={request.AssigneeUserId} не найден." }
                });
        }

        if (assignee.Role != "Engineer")
        {
            return ServiceResult<OrderResponse>.Validation(
                "Исполнителем может быть только инженер-наладчик.",
                new Dictionary<string, string[]>
                {
                    ["assigneeUserId"] = new[] { "Пользователь не является инженером-наладчиком (роль Engineer)." }
                });
        }

        // Правило процесса: у завершённого заказа исполнителя менять нельзя -> 409 Conflict
        if (item.Status == "Closed" || item.Status == "Cancelled")
        {
            return ServiceResult<OrderResponse>.Conflict(
                $"Нельзя менять исполнителя заказа в статусе {item.Status}.");
        }

        item.AssigneeUserId = request.AssigneeUserId;
        _db.SaveChanges();

        return ServiceResult<OrderResponse>.Success(MapToResponse(item));
    }

    public ServiceResult<OrderResponse> ChangeStatus(int id, ChangeStatusRequest? request)
    {
        var newStatus = (request?.Status ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(newStatus) || !AllowedStatuses.Contains(newStatus))
        {
            return ServiceResult<OrderResponse>.Validation(
                "Недопустимый статус. Допустимые значения: New, InProgress, Closed, Cancelled.",
                new Dictionary<string, string[]>
                {
                    ["status"] = new[] { "Допустимые статусы: New, InProgress, Closed, Cancelled." }
                });
        }

        var item = _db.Orders.FirstOrDefault(o => o.Id == id);
        if (item is null)
        {
            return ServiceResult<OrderResponse>.NotFound($"Заказ с id={id} не найден.");
        }

        // Правило процесса: недопустимый переход статуса -> 409 Conflict
        if (!AllowedTransitions[item.Status].Contains(newStatus))
        {
            return ServiceResult<OrderResponse>.Conflict(
                $"Нельзя перевести заказ из статуса {item.Status} в статус {newStatus}.");
        }

        // Правило процесса: нельзя переводить заказ в InProgress, пока не назначен исполнитель -> 409 Conflict
        if (newStatus == "InProgress" && item.AssigneeUserId is null)
        {
            return ServiceResult<OrderResponse>.Conflict(
                "Нельзя перевести заказ в статус InProgress, пока не назначен исполнитель (assigneeUserId).");
        }

        item.Status = newStatus;
        _db.SaveChanges();

        return ServiceResult<OrderResponse>.Success(MapToResponse(item));
    }

    private static OrderResponse MapToResponse(Order item) => new()
    {
        Id = item.Id,
        Number = item.Number,
        Title = item.Title,
        ModelId = item.ModelId,
        Description = item.Description,
        Status = item.Status,
        CreatedAt = item.CreatedAt,
        CreatedByUserId = item.CreatedByUserId,
        AssigneeUserId = item.AssigneeUserId
    };
}
