using TaskTracker.Api.Models;
using TaskTracker.Api.Requests;
using TaskTracker.Api.Services;
using Xunit;

namespace TaskTracker.Tests;

public sealed class TaskItemValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenTitleIsMissingAndStatusIsNotDone_ReturnsRequiredTitleError(string? title)
    {
        var request = new CreateTaskItemRequest
        {
            Title = title,
            Status = TaskItemStatus.Todo
        };

        var errors = TaskItemValidator.Validate(request);

        Assert.True(errors.ContainsKey("Title"));
        Assert.Contains("Title is required.", errors["Title"]);
    }

    [Fact]
    public void Validate_WhenTitleExceedsMaximumLength_ReturnsTitleLengthError()
    {
        var request = new CreateTaskItemRequest
        {
            Title = new string('a', 101),
            Status = TaskItemStatus.Todo
        };

        var errors = TaskItemValidator.Validate(request);

        Assert.True(errors.ContainsKey("Title"));
        Assert.Contains("Title must be 100 characters or fewer.", errors["Title"]);
    }

    [Fact]
    public void Validate_WhenStatusIsInvalid_ReturnsStatusError()
    {
        var request = new CreateTaskItemRequest
        {
            Title = "Valid title",
            Status = (TaskItemStatus)999
        };

        var errors = TaskItemValidator.Validate(request);

        Assert.True(errors.ContainsKey("Status"));
        Assert.Contains("Status must be one of: Todo, InProgress, Done.", errors["Status"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenTaskIsMarkedDoneWithEmptyTitle_ReturnsBusinessRuleError(string? title)
    {
        var request = new UpdateTaskItemRequest
        {
            Title = title,
            Status = TaskItemStatus.Done
        };

        var errors = TaskItemValidator.Validate(request);

        Assert.True(errors.ContainsKey("Title"));
        Assert.Contains("A task cannot be marked as Done if the Title is empty or whitespace.", errors["Title"]);
    }
}
