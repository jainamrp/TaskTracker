using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskTracker.Api.Filters;

public sealed class APIExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<APIExceptionFilterAttribute> _logger;

    public APIExceptionFilterAttribute(ILogger<APIExceptionFilterAttribute> logger)
    {
        _logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled API exception.");

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "The request could not be completed.",
            Instance = context.HttpContext.Request.Path
        };

        context.Result = new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
    }
}
