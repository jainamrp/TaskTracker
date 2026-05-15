using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using TaskTracker.Api.Models;
using TaskTracker.Api.Responses;
using Xunit;

namespace TaskTracker.Tests;

public sealed class TaskEndpointTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task PostTasks_WithInvalidTitle_ReturnsValidationProblem()
    {
        using var factory = new TaskTrackerApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/tasks", new
        {
            title = "",
            status = "Todo"
        });

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("Title", problem!.Errors.Keys);
    }

    [Fact]
    public async Task PostTasks_WhenMarkedDoneWithWhitespaceTitle_ReturnsBusinessRuleValidationProblem()
    {
        using var factory = new TaskTrackerApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/tasks", new
        {
            title = "   ",
            status = "Done"
        });

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("A task cannot be marked as Done if the Title is empty or whitespace.", problem!.Errors["Title"]);
    }

    [Fact]
    public async Task PostTasks_WithValidPayload_CreatesTask()
    {
        using var factory = new TaskTrackerApiFactory();
        using var client = factory.CreateClient();

        var dueDate = DateTime.UtcNow.Date.AddDays(7);

        var response = await client.PostAsJsonAsync("/tasks", new
        {
            title = "Write unit tests",
            description = "Cover validation and update paths",
            status = "Todo",
            dueDate
        });

        var created = await response.Content.ReadFromJsonAsync<TaskItemResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("Write unit tests", created.Title);
        Assert.Equal("Cover validation and update paths", created.Description);
        Assert.Equal(TaskItemStatus.Todo, created.Status);
        Assert.Equal(dueDate.Date, created.DueDate!.Value.Date);

        var getResponse = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task PutTasks_WithValidPayload_UpdatesTask()
    {
        using var factory = new TaskTrackerApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/tasks", new
        {
            title = "Initial title",
            description = "Initial description",
            status = "Todo"
        });

        var created = await createResponse.Content.ReadFromJsonAsync<TaskItemResponse>(JsonOptions);
        Assert.NotNull(created);

        var updateResponse = await client.PutAsJsonAsync($"/tasks/{created!.Id}", new
        {
            title = "Updated title",
            description = "Updated description",
            status = "InProgress",
            dueDate = DateTime.UtcNow.Date.AddDays(1)
        });

        var updated = await updateResponse.Content.ReadFromJsonAsync<TaskItemResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated!.Id);
        Assert.Equal("Updated title", updated.Title);
        Assert.Equal("Updated description", updated.Description);
        Assert.Equal(TaskItemStatus.InProgress, updated.Status);
    }

    [Fact]
    public async Task GetTasks_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        using var factory = new TaskTrackerApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/tasks/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTasks_WhenTaskExists_DeletesTask()
    {
        using var factory = new TaskTrackerApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/tasks", new
        {
            title = "Delete me",
            status = "Todo"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<TaskItemResponse>(JsonOptions);
        Assert.NotNull(created);

        var deleteResponse = await client.DeleteAsync($"/tasks/{created!.Id}");
        var getResponse = await client.GetAsync($"/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
