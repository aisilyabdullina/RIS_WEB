using backend.Dtos;
using backend.Models;

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

    // Допустимые переходы статусов (docs/data-model.md, раздел 6). Из Closed и Cancelled переходов нет.
    private static readonly Dictionary<string, string[]> AllowedTransitions = new(StringComparer.Ordinal)
    {
        ["New"] = new[] { "InProgress", "Cancelled" },
        ["InProgress"] = new[] { "Closed", "Cancelled" },
        ["Closed"] = Array.Empty<string>(),
        ["Cancelled"] = Array.Empty<string>()
    };

    private readonly List<Order> _items = new();
    private readonly object _sync = new();
    private int _nextId = 1;

    public OrderService()
    {
        // Стартовые записи в памяти: запись #1 намеренно в статусе New без исполнителя (для проверки 409 и двух PATCH)
        SeedItem(
            title: "Контроллеры для линии фасовки",
            modelId: 1,
            description: "10 шт., интерфейс Modbus RTU, исполнение для цеха с повышенной температурой",
            status: "New",
            assigneeUserId: null);

        SeedItem(
            title: "Модули аналогового ввода для насосной станции",
            modelId: 4,
            description: "4 шт., входы 4–20 мА, монтаж на DIN-рейку, поставка вместе с паспортами",
            status: "InProgress",
            assigneeUserId: 3);
    }

    private void SeedItem(string title, int modelId, string description, string status, int? assigneeUserId)
    {
        var id = _nextId++;
        _items.Add(new Order
        {
            Id = id,
            Number = $"ЗК-2026-{id:D4}",
            Title = title,
            ModelId = modelId,
            Description = description,
            Status = status,
            AssigneeUserId = assigneeUserId
        });
    }

    public string GetApplicationName() => "КонтрОрдер (АО «НИЦ «Инкомсистем»)";

    public string GetLabStageNote() => "ЛР8: REST главного объекта (создание, чтение, назначение и статус в памяти)";

    public IReadOnlyList<OrderResponse> GetAll()
    {
        lock (_sync)
        {
            return _items.Select(MapToResponse).ToList();
        }
    }

    public OrderResponse? GetById(int id)
    {
        lock (_sync)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            return item is null ? null : MapToResponse(item);
        }
    }

    public ServiceResult<OrderResponse> Create(CreateOrderRequest? request)
    {
        if (request is null)
        {
            return ServiceResult<OrderResponse>.Validation("Тело запроса не может быть пустым.");
        }

        var cleanTitle = (request.Title ?? string.Empty).Trim();
        var cleanDescription = (request.Description ?? string.Empty).Trim();
        var modelId = request.ResolvedModelId;

        var errors = new Dictionary<string, string[]>();

        if (cleanTitle.Length < 5 || cleanTitle.Length > 80)
        {
            errors["title"] = new[] { "Поле title должно содержать от 5 до 80 символов." };
        }

        if (modelId <= 0)
        {
            errors["modelId"] = new[] { "Поле modelId (siteId) должно быть целым числом больше нуля." };
        }

        if (cleanDescription.Length < 10 || cleanDescription.Length > 500)
        {
            errors["description"] = new[] { "Поле description должно содержать от 10 до 500 символов." };
        }

        if (errors.Count > 0)
        {
            return ServiceResult<OrderResponse>.Validation("Проверьте поля запроса.", errors);
        }

        lock (_sync)
        {
            var id = _nextId++;
            var created = new Order
            {
                Id = id,
                Number = $"ЗК-2026-{id:D4}",
                Title = cleanTitle,
                ModelId = modelId,
                Description = cleanDescription,
                Status = "New",
                AssigneeUserId = null
            };

            _items.Add(created);
            return ServiceResult<OrderResponse>.Success(MapToResponse(created));
        }
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

        lock (_sync)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item is null)
            {
                return ServiceResult<OrderResponse>.NotFound($"Заказ с id={id} не найден.");
            }

            // Правило процесса: у завершённого заказа (Closed / Cancelled) исполнителя менять нельзя -> 409 Conflict
            if (item.Status == "Closed" || item.Status == "Cancelled")
            {
                return ServiceResult<OrderResponse>.Conflict(
                    $"Нельзя менять исполнителя заказа в статусе {item.Status}.");
            }

            item.AssigneeUserId = request.AssigneeUserId;
            return ServiceResult<OrderResponse>.Success(MapToResponse(item));
        }
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

        lock (_sync)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
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
            return ServiceResult<OrderResponse>.Success(MapToResponse(item));
        }
    }

    private static OrderResponse MapToResponse(Order item) => new()
    {
        Id = item.Id,
        Number = item.Number,
        Title = item.Title,
        ModelId = item.ModelId,
        SiteId = item.ModelId,
        Description = item.Description,
        Status = item.Status,
        AssigneeUserId = item.AssigneeUserId
    };
}
