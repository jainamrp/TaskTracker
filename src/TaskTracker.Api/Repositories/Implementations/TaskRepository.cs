using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Repositories.Implementations;

public sealed class TaskRepository : ITaskRepository
{
    private readonly TaskTrackerDbContext _db;

    public TaskRepository(TaskTrackerDbContext db)
    {
        _db = db;
    }

    public async Task<TaskItem?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _db.Tasks.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken cancellationToken = default) =>
        await GetAllAsync(cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Tasks
            .AsNoTracking()
            .OrderBy(task => task.Id)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TaskItem entity, CancellationToken cancellationToken = default)
    {
        await _db.Tasks.AddAsync(entity, cancellationToken);
    }

    public void Remove(TaskItem entity)
    {
        _db.Tasks.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}
