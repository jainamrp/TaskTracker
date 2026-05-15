using TaskTracker.Api.Models;
using TaskTracker.Api.Requests;
using TaskTracker.Api.Responses;

namespace TaskTracker.Api.Mapping;

public static class TaskItemMappings
{
    public static TaskItem ToEntity(this CreateTaskItemRequest request) => new()
    {
        Title = request.Title!.Trim(),
        Description = request.Description,
        Status = request.Status,
        DueDate = request.DueDate
    };

    public static void Apply(this TaskItem task, UpdateTaskItemRequest request)
    {
        task.Title = request.Title!.Trim();
        task.Description = request.Description;
        task.Status = request.Status;
        task.DueDate = request.DueDate;
    }

    public static TaskItemResponse ToResponse(this TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.DueDate);
}
