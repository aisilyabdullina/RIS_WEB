namespace backend.Dtos;

/// <summary>
/// Входной DTO для отдельного назначения исполнителя: PATCH /api/orders/{id}/assignee
/// </summary>
public class AssignAssigneeRequest
{
    public int AssigneeUserId { get; set; }
}
