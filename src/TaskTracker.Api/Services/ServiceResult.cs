namespace TaskTracker.Api.Services;

public enum ServiceResultStatus
{
    Success,
    NotFound,
    ValidationFailed
}

public sealed class ServiceResult
{
    private ServiceResult(ServiceResultStatus status, Dictionary<string, string[]>? errors = null)
    {
        Status = status;
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ServiceResultStatus Status { get; }
    public Dictionary<string, string[]> Errors { get; }
    public bool IsSuccess => Status == ServiceResultStatus.Success;

    public static ServiceResult Success() => new(ServiceResultStatus.Success);

    public static ServiceResult NotFound() => new(ServiceResultStatus.NotFound);

    public static ServiceResult ValidationFailure(Dictionary<string, string[]> errors) =>
        new(ServiceResultStatus.ValidationFailed, errors);
}

public sealed class ServiceResult<T>
{
    private ServiceResult(ServiceResultStatus status, T? value = default, Dictionary<string, string[]>? errors = null)
    {
        Status = status;
        Value = value;
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ServiceResultStatus Status { get; }
    public T? Value { get; }
    public Dictionary<string, string[]> Errors { get; }
    public bool IsSuccess => Status == ServiceResultStatus.Success;

    public static ServiceResult<T> Success(T value) => new(ServiceResultStatus.Success, value);

    public static ServiceResult<T> NotFound() => new(ServiceResultStatus.NotFound);

    public static ServiceResult<T> ValidationFailure(Dictionary<string, string[]> errors) =>
        new(ServiceResultStatus.ValidationFailed, errors: errors);
}
