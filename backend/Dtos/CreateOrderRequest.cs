namespace backend.Dtos;

/// <summary>
/// Входной DTO для создания заказа: клиент передаёт только изменяемые человеком поля.
/// Поля id, number, status и assigneeUserId формирует сервер.
/// </summary>
public class CreateOrderRequest
{
    public string? Title { get; set; }
    public int? ModelId { get; set; }
    public string? Description { get; set; }
    public int? CreatedByUserId { get; set; }
}
