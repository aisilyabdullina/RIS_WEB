namespace backend.Models;

/// <summary>
/// Справочник моделей контроллеров (таблица controller_models в PostgreSQL).
/// </summary>
public class ControllerModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public string ControllerType { get; set; } = string.Empty;
    public int IoCount { get; set; }
    public string Protocol { get; set; } = string.Empty;
    public int StockQty { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
