# Routr

**A lightweight, zero‑reflection Mediator implementation for .NET, powered by Roslyn source generators.**

Routr eliminates runtime reflection by generating fast, trim‑friendly dispatch code at compile time.  
It supports requests, notifications, and pipeline behaviours—all with a clean, minimal API.

---

## ✨ Features

- ⚡ **Zero reflection** – all handler resolution and dispatch is generated at compile time
- 🧩 **Pipeline behaviours** – middleware that wraps handler execution (logging, validation, etc.)
- 📬 **Requests & Notifications** – `IRequest<TResponse>`, `IRequest` (void with `Unit`), `INotification`
- 🚦 **Cancellation token propagation** – passed seamlessly from sender to handler
- 🧵 **Dependency injection friendly** – registers everything via `AddRoutrHandlers()`
- 🧪 **Comprehensive tests** – unit, snapshot, and integration tests prove correctness
- 📦 **AOT/trimming safe** – no reflection, no dynamic code; ideal for Native AOT

---

## 📦 Installation

Routr is distributed as two NuGet packages:

- `Routr` – core abstractions (interfaces and the optional manual `Sender`)
- `Routr.SourceGenerator` – the Roslyn incremental generator that creates the real dispatcher

To use it, add the core package to your application, and reference the generator as an **analyzer** in any project that contains handlers.

**Via CLI:**

\`\`\`bash
dotnet add package Routr
dotnet add package Routr.SourceGenerator
\`\`\`

Then ensure the generator runs as an analyzer in your handler project (e.g., Web API). If using project references, set:

\`\`\`xml
<ProjectReference Include="..\path\to\Routr.SourceGenerator.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
\`\`\`

---

## 🚀 Quick Start

### 1. Define a request and its handler

\`\`\`csharp
using Routr;

public record CreateUserCommand(string Name) : IRequest<Guid>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    public Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid());
}
\`\`\`

### 2. Register Routr in your DI container

\`\`\`csharp
// Program.cs
builder.Services.AddRoutrHandlers();   // This method is source‑generated!
\`\`\`

### 3. Inject `ISender` and dispatch

\`\`\`csharp
app.MapPost("/users", async (CreateUserCommand cmd, ISender sender) =>
{
    var newId = await sender.Send(cmd);
    return Results.Created($"/users/{newId}", new { Id = newId });
});
\`\`\`

That's it—no marker interfaces, no base classes, no reflection.

---

## 🧩 Pipeline Behaviours

Behaviours allow cross‑cutting concerns like logging, validation, or retries to be applied to requests.

**Create a behaviour:**

\`\`\`csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct)
    {
        _logger.LogInformation("Handling {Request}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {Request}", typeof(TRequest).Name);
        return response;
    }
}
\`\`\`

**Register the behaviour (order matters):**

\`\`\`csharp
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
\`\`\`

The generated sender will execute behaviours in registration order (outermost first). This is identical to MediatR's pipeline semantics.

---

## 🔔 Notifications

For fire‑and‑forget events, implement `INotification` and `INotificationHandler<T>`.

\`\`\`csharp
public record UserCreatedEvent(Guid UserId) : INotification;

public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedEvent>
{
    public Task Handle(UserCreatedEvent notification, CancellationToken ct)
    {
        // send email
        return Task.CompletedTask;
    }
}
\`\`\`

Publish via `IPublisher`:

\`\`\`csharp
await publisher.Publish(new UserCreatedEvent(newId));
\`\`\`

All registered handlers for that notification type are discovered and called at compile time.

---

## ⚡ Performance

Routr replaces runtime reflection with compile‑time code generation, drastically reducing dispatch overhead.

| Sender                | Mean      | Allocated |
|-----------------------|----------:|----------:|
| Reflection‑based      | 178.9 ns  | 320 B     |
| **Routr (generated)** | **101.5 ns** | **352 B** |

*Benchmarked with [BenchmarkDotNet](https://benchmarkdotnet.net) on .NET 8, Intel Core i5‑12450HX, using a simple request/handler pair.  
The generated sender is **1.8× faster** while maintaining comparable memory usage. The slight allocation increase is due to pipeline readiness overhead and can be eliminated when no behaviours are registered.*

---

## ⚖️ Comparison with MediatR

| Feature | MediatR | Routr |
|--------|---------|-------|
| Reflection‑based | Yes | **No** |
| Source‑generated dispatch | No | **Yes** |
| AOT / Native AOT compatible | Requires configuration | **Fully compatible** |
| Pipeline behaviours | Yes | Yes |
| Notifications | Yes | Yes |
| Open source | Yes (Apache 2.0) | Yes (MIT) |

Routr focuses on simplicity and performance. If you don't need the extra features of MediatR, Routr gives you the same pattern with zero overhead.

---

## 🧪 Testing

Routr's test suite ensures correctness at every level:

- **Unit tests** for core abstractions (`Unit`, handler contracts).
- **Snapshot tests** (using [Verify](https://github.com/VerifyTests/Verify)) to lock down the generated C# source code.
- **Integration tests** that exercise the *exact* generated sender/publisher through a real DI container, covering pipeline execution, cancellation, exception propagation, and notification dispatch.
- **Benchmarks** to measure and compare performance.

All tests run in CI and can be executed locally:

\`\`\`bash
dotnet test
\`\`\`

---

## 🤝 Contributing

Contributions are welcome! Please open an issue or a pull request.  
To get started:

1. Clone the repository.
2. Open the solution in your favourite IDE.
3. Run the tests: `dotnet test`.
4. Make your changes and ensure all tests pass.

---

## 📄 License

Routr is licensed under the [MIT License](LICENSE).  
Feel free to use it in personal and commercial projects.

---

Made with ❤️ and C# source generators.
