---
name: "FastEndpoints FluentResults Pattern"
description: "How to structure endpoint responses using FluentResults and Wolverine MessageBus"
---

# FastEndpoints FluentResults Pattern

When building API endpoints using FastEndpoints in this project, always adhere to the following pattern for returning responses:

1. **Handlers**: 
   - Handlers (invoked via Wolverine `IMessageBus`) should always return `Result<T>` or `Result` from the `FluentResults` library.
   - Example: `public async Task<Result<Guid>> Handle(...)`

2. **Endpoints**:
   - Endpoints should inherit from `Endpoint<TRequest, TResponse>` where `TResponse` is the successful return type (e.g., `Guid`, `UserDto`).
   - Use `await bus.InvokeAsync<Result<TResponse>>(req, ct)` to invoke the handler.
   - Return the HTTP response using the extension method `await this.SendResultAsync(result, ct);` from `SharedKernel.Extensions.Results`.
   - Never use `WriteAsJsonAsync` or `SendAsync` directly when the handler returns a `Result`.

## Example

```csharp
using FastEndpoints;
using FluentResults;
using Wolverine;
using SharedKernel.Extensions.Results;

namespace MyModule.Features.Users.CreateUser;

internal class CreateUserEndpoint(IMessageBus bus) : Endpoint<CreateUserCommand, Guid>
{
    public override void Configure()
    {
        Post("/");
        Group<UsersGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateUserCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<Guid>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
```
