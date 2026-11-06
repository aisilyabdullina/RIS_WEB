namespace backend.Models;

/// <summary>
/// Серверная модель заказа на контроллер (главный объект процесса).
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int ModelId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public int? AssigneeUserId { get; set; }
}
