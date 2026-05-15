using TaskTracker.Api.Models;

namespace TaskTracker.Api.Requests;

public sealed class UpdateTaskItemRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public TaskItemStatus Status { get; init; } = TaskItemStatus.Todo;
    public DateTime? DueDate { get; init; }
}
