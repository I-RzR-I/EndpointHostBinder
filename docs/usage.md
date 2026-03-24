## Endpoint model

An endpoint is defined by the following properties:

| Property | Description |
|---|---|
| `Name` | Display name used in logging and diagnostics. |
| `Path` | URL path the endpoint is mapped to (e.g. `/api/my-endpoint`). |
| `IsActive` | Whether the endpoint handles incoming requests (`true`) or is skipped (`false`). |
| `EndpointType` | The `IEndpointHostRequestHandler` implementation type. |
| `AllowedMethods` | HTTP methods the endpoint accepts. `null` / empty means all methods are accepted. |
| `Executor` | Pre-compiled delegate used by the middleware to dispatch requests. Set automatically during registration. |

---

## Setup

In `Startup.cs` (or `Program.cs` for minimal hosting), add the following:

```csharp
// ConfigureServices / builder.Services
services.RegisterEndpointHostBuilder();
```

```csharp
// Configure / app pipeline
app.UseEndpointHostBuilder();
```

> `RegisterEndpointHostBuilder` must be called before any `AddHostEndpoint` / `AddHostEndpointsFromAssembly` registration, and `UseEndpointHostBuilder` should be placed before `UseEndpoints` / `MapControllers`.

---

## Implementing a handler

Create a class implementing `IEndpointHostRequestHandler`. This is where request validation and processing logic lives.

```csharp
public class SysTimeEndpointHandler : IEndpointHostRequestHandler
{
    /// <inheritdoc />
    public Task<IEndpointHostResult> RequestProcessAsync(
        HttpContext context, CancellationToken cancellationToken = default)
    {
        IEndpointHostResult result = HttpMethods.IsGet(context.Request.Method)
            ? new SysTimeEndpointResult()
            : new StatusCodeResult(HttpStatusCode.MethodNotAllowed);

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public IEndpointHostResult RequestProcess(HttpContext context)
    {
        return HttpMethods.IsGet(context.Request.Method)
            ? new SysTimeEndpointResult()
            : new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
    }
}
```

## Implementing a result

Create a class implementing `IEndpointHostResult`. This is responsible for writing the HTTP response.

```csharp
public class SysTimeEndpointResult : IEndpointHostResult
{
    /// <inheritdoc />
    public Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        Execute(context);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Execute(HttpContext context)
        => context.Response.WriteAsync($"{DateTime.Now}");
}
```

---

## Registering endpoints

### Option 1 — Manual registration

Useful when you need full control over the endpoint name, path, active state, or HTTP method constraints.

```csharp
services.RegisterEndpointHostBuilder();

// Basic — responds to all HTTP methods
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime");

// With active flag
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime", isActive: true);

// With HTTP method constraints  
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime",
    new[] { HttpMethod.Get });

// With active flag and HTTP method constraints
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime",
    isActive: true, new[] { HttpMethod.Get, HttpMethod.Post });

// With a pre-configured Endpoint instance
services.AddHostEndpoint<SysTimeEndpointHandler>(new Endpoint(
    "sysTime", "/systime", typeof(SysTimeEndpointHandler), isActive: true));
```

### Option 2 — Attribute-driven registration (auto-discovery)

Decorate your handler with `[EndpointHost]` and call `AddHostEndpointsFromAssembly`. The library will scan the assembly for all decorated handlers and register them automatically.

```csharp
// 1. Decorate the handler
[EndpointHost("sysTime2", "/systime2", isActive: true, "GET")]
public class SysTimeEndpointHandler2 : IEndpointHostRequestHandler
{
    // ...
}
```

```csharp
// 2. Register all discovered endpoints
services.RegisterEndpointHostBuilder()
    .AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly());
```

### Mixing both approaches

`AddHostEndpointsFromAssembly` automatically skips handlers that were already registered manually, so both approaches can be combined safely.

```csharp
services.RegisterEndpointHostBuilder()
    .AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly()); // skips duplicates

services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime"); // manual override
```

---

## Startup code example

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.RegisterEndpointHostBuilder()
        .AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly());

    // Optionally register additional endpoints manually
    services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime");

    services.AddControllers();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRouting();
    app.UseEndpointHostBuilder(); // must come before UseEndpoints
    app.UseEndpoints(endpoints => endpoints.MapControllers());
}
```

