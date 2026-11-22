namespace backend.Models;

/// <summary>
/// Главный объект — заказ на контроллер (таблица orders в PostgreSQL).
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ModelId { get; set; }
    public string Status { get; set; } = "New";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int CreatedByUserId { get; set; } = 1;
    public int? AssigneeUserId { get; set; }

    public ControllerModel? ControllerModel { get; set; }
    public User? CreatedByUser { get; set; }
    public User? AssigneeUser { get; set; }
}
