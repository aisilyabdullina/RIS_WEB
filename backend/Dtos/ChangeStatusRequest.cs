namespace backend.Dtos;

/// <summary>
/// Входной DTO для отдельной смены статуса: PATCH /api/orders/{id}/status
/// </summary>
public class ChangeStatusRequest
{
    public string? Status { get; set; }
}
