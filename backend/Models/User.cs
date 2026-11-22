namespace backend.Models;

/// <summary>
/// Пользователь системы: создатель заказа или исполнитель (таблица users в PostgreSQL).
/// </summary>
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Engineer";

    public ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
    public ICollection<Order> AssignedOrders { get; set; } = new List<Order>();
}
