namespace backend.Dtos;

/// <summary>
/// Выходной DTO заказа, возвращаемый клиенту.
/// Содержит серверные поля id, number, status и assigneeUserId.
/// </summary>
public class OrderResponse
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int ModelId { get; set; }
    public int SiteId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public int? AssigneeUserId { get; set; }
}
