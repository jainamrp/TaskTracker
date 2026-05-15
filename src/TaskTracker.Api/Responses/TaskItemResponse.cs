using TaskTracker.Api.Models;

namespace TaskTracker.Api.Responses;

public sealed record TaskItemResponse(
    int Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTime? DueDate);
