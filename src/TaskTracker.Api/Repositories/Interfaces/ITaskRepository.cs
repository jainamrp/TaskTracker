using TaskTracker.Api.Models;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
}
