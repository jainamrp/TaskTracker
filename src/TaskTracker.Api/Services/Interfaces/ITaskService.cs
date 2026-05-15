using TaskTracker.Api.Requests;
using TaskTracker.Api.Responses;

namespace TaskTracker.Api.Services.Interfaces;

public interface ITaskService
{
    Task<ServiceResult<TaskItemResponse>> CreateTaskAsync(
        CreateTaskItemRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItemResponse>> GetTasksAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<TaskItemResponse>> GetTaskByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<TaskItemResponse>> UpdateTaskAsync(
        int id,
        UpdateTaskItemRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
}
