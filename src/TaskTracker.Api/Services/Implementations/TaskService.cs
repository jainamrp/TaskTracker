using TaskTracker.Api.Mapping;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Requests;
using TaskTracker.Api.Responses;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Services.Implementations;

public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<ServiceResult<TaskItemResponse>> CreateTaskAsync(
        CreateTaskItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = TaskItemValidator.Validate(request);
        if (errors.Count > 0)
        {
            return ServiceResult<TaskItemResponse>.ValidationFailure(errors);
        }

        var task = request.ToEntity();
        await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<TaskItemResponse>.Success(task.ToResponse());
    }

    public async Task<IReadOnlyList<TaskItemResponse>> GetTasksAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken);

        return tasks.Select(task => task.ToResponse()).ToList();
    }

    public async Task<ServiceResult<TaskItemResponse>> GetTaskByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.FindByIdAsync(id, cancellationToken);

        return task is null
            ? ServiceResult<TaskItemResponse>.NotFound()
            : ServiceResult<TaskItemResponse>.Success(task.ToResponse());
    }

    public async Task<ServiceResult<TaskItemResponse>> UpdateTaskAsync(
        int id,
        UpdateTaskItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = TaskItemValidator.Validate(request);
        if (errors.Count > 0)
        {
            return ServiceResult<TaskItemResponse>.ValidationFailure(errors);
        }

        var task = await _taskRepository.FindByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return ServiceResult<TaskItemResponse>.NotFound();
        }

        task.Apply(request);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<TaskItemResponse>.Success(task.ToResponse());
    }

    public async Task<ServiceResult> DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.FindByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return ServiceResult.NotFound();
        }

        _taskRepository.Remove(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }
}
