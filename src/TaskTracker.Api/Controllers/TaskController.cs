using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.Requests;
using TaskTracker.Api.Responses;
using TaskTracker.Api.Services;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.TaskEndpoints;

[ApiController]
[Route("tasks")]
public sealed class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemResponse>> CreateTaskAsync(
        [FromBody] CreateTaskItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateTaskAsync(request, cancellationToken);

        return result.Status switch
        {
            ServiceResultStatus.Success => Created($"/tasks/{result.Value!.Id}", result.Value),
            ServiceResultStatus.ValidationFailed => BadRequest(CreateValidationProblem(result.Errors)),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskItemResponse>>> GetTasksAsync(CancellationToken cancellationToken)
    {
        var response = await _taskService.GetTasksAsync(cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItemResponse>> GetTaskByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetTaskByIdAsync(id, cancellationToken);

        return ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskItemResponse>> UpdateTaskAsync(
        int id,
        [FromBody] UpdateTaskItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateTaskAsync(id, request, cancellationToken);

        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTaskAsync(int id, CancellationToken cancellationToken)
    {
        var result = await _taskService.DeleteTaskAsync(id, cancellationToken);

        return result.Status switch
        {
            ServiceResultStatus.Success => NoContent(),
            ServiceResultStatus.NotFound => NotFound(),
            ServiceResultStatus.ValidationFailed => BadRequest(CreateValidationProblem(result.Errors)),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private ActionResult<TaskItemResponse> ToActionResult(ServiceResult<TaskItemResponse> result) =>
        result.Status switch
        {
            ServiceResultStatus.Success => Ok(result.Value),
            ServiceResultStatus.NotFound => NotFound(),
            ServiceResultStatus.ValidationFailed => BadRequest(CreateValidationProblem(result.Errors)),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };

    private ValidationProblemDetails CreateValidationProblem(Dictionary<string, string[]> errors) => new(errors)
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "One or more validation errors occurred.",
        Instance = HttpContext.Request.Path
    };
}
