namespace backend.Dtos;

/// <summary>
/// Входной DTO для создания заказа на контроллер (ЛР8).
/// Клиент передаёт только изменяемые человеком поля (без id, number, status и assigneeUserId).
/// Поддерживаются оба имени поля справочника: modelId (из ЛР2) и siteId (из шаблона методички).
/// </summary>
public class CreateOrderRequest
{
    public string? Title { get; set; }
    public int? ModelId { get; set; }
    public int? SiteId { get; set; }
    public string? Description { get; set; }

    public int ResolvedModelId => ModelId ?? SiteId ?? 0;
}
