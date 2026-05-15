using TaskTracker.Api.Models;
using TaskTracker.Api.Requests;

namespace TaskTracker.Api.Services;

public static class TaskItemValidator
{
    private const int MaxTitleLength = 100;
    private const string RequiredTitleMessage = "Title is required.";
    private const string MaxTitleLengthMessage = "Title must be 100 characters or fewer.";
    private const string InvalidStatusMessage = "Status must be one of: Todo, InProgress, Done.";
    private const string DoneRequiresTitleMessage = "A task cannot be marked as Done if the Title is empty or whitespace.";

    public static Dictionary<string, string[]> Validate(CreateTaskItemRequest request) =>
        Validate(request.Title, request.Status);

    public static Dictionary<string, string[]> Validate(UpdateTaskItemRequest request) =>
        Validate(request.Title, request.Status);

    private static Dictionary<string, string[]> Validate(string? title, TaskItemStatus status)
    {
        var errors = new Dictionary<string, List<string>>();
        var isDefinedStatus = Enum.IsDefined(typeof(TaskItemStatus), status);
        var normalizedTitle = title?.Trim();

        if (!isDefinedStatus)
        {
            Add(errors, nameof(CreateTaskItemRequest.Status), InvalidStatusMessage);
        }

        if (isDefinedStatus && status == TaskItemStatus.Done && string.IsNullOrWhiteSpace(title))
        {
            Add(errors, nameof(CreateTaskItemRequest.Title), DoneRequiresTitleMessage);
        }
        else if (string.IsNullOrWhiteSpace(title))
        {
            Add(errors, nameof(CreateTaskItemRequest.Title), RequiredTitleMessage);
        }
        else if (normalizedTitle!.Length > MaxTitleLength)
        {
            Add(errors, nameof(CreateTaskItemRequest.Title), MaxTitleLengthMessage);
        }

        return errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
    }

    private static void Add(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = new List<string>();
            errors[key] = messages;
        }

        messages.Add(message);
    }
}
