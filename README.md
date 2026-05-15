# Task Tracker API

Task Tracker API is a small ASP.NET Core Web API for creating, reading, updating, and deleting task items. It uses a layered architecture with controllers, services, validators, repositories, Entity Framework Core, and SQLite persistence.

## Tech Stack

- .NET 8
- ASP.NET Core MVC controllers
- Entity Framework Core with SQLite
- xUnit integration and unit tests
- `WebApplicationFactory<Program>` for endpoint tests
- Coverlet collector for code coverage

## Solution Structure

```text
TaskTracker.sln
src/
  TaskTracker.Api/
    Controllers/              HTTP endpoints
    Data/                     EF Core DbContext
    Filters/                  API filters and request logging middleware
    Mapping/                  Request/entity/response mapping extensions
    Models/                   Domain model and enum
    Repositories/             Data access interfaces and implementations
    Requests/                 Incoming API DTOs
    Responses/                Outgoing API DTOs
    Services/                 Business logic, validation, and service results
    Program.cs                App configuration and request pipeline
tests/
  TaskTracker.Tests/
    TaskEndpointTests.cs      End-to-end API behavior tests
    TaskItemValidatorTests.cs Validator unit tests
    TaskTrackerApiFactory.cs  Test host with in-memory SQLite
```

## Architecture

The API follows a simple layered flow:

```text
HTTP request
  -> MessageHandler middleware
  -> MVC routing
  -> ValidateModelAttribute
  -> TaskController
  -> ITaskService / TaskService
  -> TaskItemValidator and mapping extensions
  -> ITaskRepository / TaskRepository
  -> TaskTrackerDbContext
  -> SQLite
```

Responsibilities are separated as follows:

- `Controllers`: receive HTTP requests, call services, and translate `ServiceResult` values into HTTP responses.
- `Services`: coordinate validation, mapping, repository calls, and business outcomes.
- `Repositories`: isolate EF Core data access from services and controllers.
- `Models`: define the persisted domain shape.
- `Requests`: define incoming JSON payloads.
- `Responses`: define outgoing JSON payloads.
- `Mapping`: converts between requests, entities, and responses.
- `Filters`: provide cross-cutting behavior for validation, exception handling, and request logging.
- `Data`: owns `TaskTrackerDbContext`.

## Application Configuration

`Program.cs` configures the app:

- Adds MVC controllers.
- Adds global filters:
  - `APIExceptionFilterAttribute`
  - `ValidateModelAttribute`
- Adds `JsonStringEnumConverter` so enum values are serialized and read as strings such as `"Todo"` and `"Done"`.
- Suppresses ASP.NET Core's default model-state invalid filter so the custom validation filter controls validation response formatting.
- Registers `TaskTrackerDbContext` with SQLite.
- Registers scoped dependencies:
  - `ITaskRepository -> TaskRepository`
  - `ITaskService -> TaskService`
- Calls `Database.EnsureCreated()` on startup.
- Adds `MessageHandler` middleware.
- Maps controller routes.

`appsettings.json` contains the database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tasks.db"
  }
}
```

If the connection string is missing, `Program.cs` falls back to `Data Source=tasks.db`.

## Middleware and Filters

### MessageHandler

`Filters/MessageHandler.cs` is custom middleware. It wraps every request, measures elapsed time, and logs:

```text
HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms.
```

### ValidateModelAttribute

`Filters/ValidateModelAttribute.cs` runs before controller actions. If MVC model binding adds model-state errors, it returns `400 Bad Request` with `ValidationProblemDetails`.

### APIExceptionFilterAttribute

`Filters/APIExceptionFilterAttribute.cs` catches unhandled controller exceptions, logs the exception, and returns `500 Internal Server Error` with `ProblemDetails`.

## Models

### TaskItem

`Models/TaskItem.cs` is the EF Core entity:

```json
{
  "id": 1,
  "title": "Write tests",
  "description": "Cover validation and update paths",
  "status": "Todo",
  "dueDate": "2026-05-19T00:00:00Z"
}
```

Fields:

- `Id`: database-generated integer key.
- `Title`: required by service validation and trimmed before storage.
- `Description`: optional text.
- `Status`: task status enum.
- `DueDate`: optional date/time.

### TaskItemStatus

Allowed values:

- `Todo`
- `InProgress`
- `Done`

Enums are serialized as strings in API JSON.

## Requests

### CreateTaskItemRequest

Used by `POST /tasks`.

```json
{
  "title": "Write unit tests",
  "description": "Cover validation and update paths",
  "status": "Todo",
  "dueDate": "2026-05-19T00:00:00Z"
}
```

### UpdateTaskItemRequest

Used by `PUT /tasks/{id}`.

```json
{
  "title": "Updated title",
  "description": "Updated description",
  "status": "InProgress",
  "dueDate": null
}
```

For both request types:

- `title` may bind as null, but service validation rejects null, empty, or whitespace values.
- `description` is optional.
- `status` defaults to `Todo` if omitted.
- `dueDate` is optional.

## Responses

### TaskItemResponse

Returned by create, get, and update operations:

```json
{
  "id": 1,
  "title": "Write unit tests",
  "description": "Cover validation and update paths",
  "status": "Todo",
  "dueDate": "2026-05-19T00:00:00Z"
}
```

Validation failures return `ValidationProblemDetails`:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "instance": "/tasks",
  "errors": {
    "Title": [
      "Title is required."
    ]
  }
}
```

Unhandled exceptions return `ProblemDetails`:

```json
{
  "title": "An unexpected error occurred.",
  "status": 500,
  "detail": "The request could not be completed.",
  "instance": "/tasks"
}
```

## Endpoints

### Create Task

`POST /tasks`

Success response: `201 Created`

```bash
curl -X POST http://localhost:62048/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Write unit tests",
    "description": "Cover validation and update paths",
    "status": "Todo",
    "dueDate": "2026-05-19T00:00:00Z"
  }'
```

Possible responses:

- `201 Created` with `TaskItemResponse`
- `400 Bad Request` with `ValidationProblemDetails`
- `500 Internal Server Error` with `ProblemDetails`

### Get All Tasks

`GET /tasks`

Success response: `200 OK`

```bash
curl http://localhost:62048/tasks
```

Returns an array of `TaskItemResponse` objects ordered by `Id`.

### Get Task By Id

`GET /tasks/{id}`

```bash
curl http://localhost:62048/tasks/1
```

Possible responses:

- `200 OK` with `TaskItemResponse`
- `404 Not Found`
- `500 Internal Server Error` with `ProblemDetails`

### Update Task

`PUT /tasks/{id}`

```bash
curl -X PUT http://localhost:62048/tasks/1 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated title",
    "description": "Updated description",
    "status": "InProgress",
    "dueDate": null
  }'
```

Possible responses:

- `200 OK` with `TaskItemResponse`
- `400 Bad Request` with `ValidationProblemDetails`
- `404 Not Found`
- `500 Internal Server Error` with `ProblemDetails`

### Delete Task

`DELETE /tasks/{id}`

```bash
curl -X DELETE http://localhost:62048/tasks/1
```

Possible responses:

- `204 No Content`
- `404 Not Found`
- `500 Internal Server Error` with `ProblemDetails`

## Services

### ITaskService

`Services/Interfaces/ITaskService.cs` defines the application use cases:

- `CreateTaskAsync`
- `GetTasksAsync`
- `GetTaskByIdAsync`
- `UpdateTaskAsync`
- `DeleteTaskAsync`

### TaskService

`Services/Implementations/TaskService.cs` implements the use cases:

- Validates create and update requests with `TaskItemValidator`.
- Converts valid create requests to `TaskItem` entities.
- Applies update requests to existing `TaskItem` entities.
- Converts persisted entities to `TaskItemResponse`.
- Returns `ServiceResult` instead of throwing for expected outcomes such as validation failure or not found.

### ServiceResult

`Services/ServiceResult.cs` standardizes service outcomes:

- `Success`
- `NotFound`
- `ValidationFailed`

The controller converts these service statuses into HTTP status codes.

## Repository Layer

### IRepository<TEntity>

The generic repository interface defines:

- `FindByIdAsync`
- `ListAsync`
- `AddAsync`
- `Remove`
- `SaveChangesAsync`

### ITaskRepository

Adds task-specific access through `GetAllAsync`.

### TaskRepository

`Repositories/Implementations/TaskRepository.cs` uses `TaskTrackerDbContext` and EF Core:

- `FindByIdAsync` uses `FindAsync`.
- `GetAllAsync` returns tasks with `AsNoTracking()`, ordered by `Id`.
- `AddAsync` adds a new task.
- `Remove` deletes a task.
- `SaveChangesAsync` commits changes.

## Data Access

`Data/TaskTrackerDbContext.cs` exposes:

```csharp
public DbSet<TaskItem> Tasks => Set<TaskItem>();
```

The application uses SQLite. The database file is `tasks.db` when running locally.

For this project, startup uses `Database.EnsureCreated()`

## Validation Rules

Validation lives in `Services/TaskItemValidator.cs`.

Rules:

- `Title` is required.
- `Title` must not be null, empty, or whitespace.
- `Title` must be 100 characters or fewer after trimming.
- `Status` must be one of `Todo`, `InProgress`, or `Done`.
- A task cannot be marked as `Done` if `Title` is empty or whitespace.

Validation messages:

- `Title is required.`
- `Title must be 100 characters or fewer.`
- `Status must be one of: Todo, InProgress, Done.`
- `A task cannot be marked as Done if the Title is empty or whitespace.`

## Request Flow

### Create Flow

```text
POST /tasks
  -> MessageHandler logs timing
  -> ValidateModelAttribute checks model binding
  -> TaskController.CreateTaskAsync
  -> TaskService.CreateTaskAsync
  -> TaskItemValidator.Validate
  -> TaskItemMappings.ToEntity
  -> TaskRepository.AddAsync
  -> TaskRepository.SaveChangesAsync
  -> TaskItemMappings.ToResponse
  -> 201 Created
```

### Read Flow

```text
GET /tasks/{id}
  -> TaskController.GetTaskByIdAsync
  -> TaskService.GetTaskByIdAsync
  -> TaskRepository.FindByIdAsync
  -> 200 OK with response, or 404 Not Found
```

### Update Flow

```text
PUT /tasks/{id}
  -> TaskController.UpdateTaskAsync
  -> TaskService.UpdateTaskAsync
  -> TaskItemValidator.Validate
  -> TaskRepository.FindByIdAsync
  -> TaskItemMappings.Apply
  -> TaskRepository.SaveChangesAsync
  -> 200 OK with response, 400 Bad Request, or 404 Not Found
```

### Delete Flow

```text
DELETE /tasks/{id}
  -> TaskController.DeleteTaskAsync
  -> TaskService.DeleteTaskAsync
  -> TaskRepository.FindByIdAsync
  -> TaskRepository.Remove
  -> TaskRepository.SaveChangesAsync
  -> 204 No Content, or 404 Not Found
```

## Unit Test Coverage and Validations

The current suite contains 14 passing tests.

Measured with `dotnet test --collect:"XPlat Code Coverage"`:

- Line coverage: 82.13% (`239 / 291` lines)
- Branch coverage: 73.91% (`34 / 46` branches)

Validator tests cover:

- Null title returns `Title is required.`
- Empty title returns `Title is required.`
- Whitespace title returns `Title is required.`
- Title longer than 100 characters returns `Title must be 100 characters or fewer.`
- Invalid enum value returns `Status must be one of: Todo, InProgress, Done.`
- `Done` status with null, empty, or whitespace title returns the business-rule error.

Endpoint tests cover:

- `POST /tasks` with invalid title returns `400 Bad Request` and validation errors.
- `POST /tasks` with `Done` and whitespace title returns business-rule validation.
- `POST /tasks` with valid payload returns `201 Created`, a location header, and the created task.
- Created task can be fetched through the returned location.
- `PUT /tasks/{id}` with valid payload updates title, description, status, and due date.
- `GET /tasks/{id}` for a missing task returns `404 Not Found`.
- `DELETE /tasks/{id}` for an existing task returns `204 No Content`.
- Deleted task returns `404 Not Found` when fetched again.

The test host uses `TaskTrackerApiFactory`, which replaces the production SQLite file database with an in-memory SQLite connection for isolated test runs.