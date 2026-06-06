# Using EndpointHostBinder

## Endpoint model

An endpoint is defined by the following properties:

| Property | Type | Description |
| --- | --- | --- |
| `Name` | `string` | Display name used in logging and diagnostics. |
| `Path` | `PathString` | URL path the endpoint is mapped to (e.g. `/api/my-endpoint`). Supports named parameters (e.g. `/users/{id}`). |
| `IsActive` | `bool` | When `false` the endpoint is skipped during matching. |
| `EndpointType` | `Type` | The `IEndpointHostRequestHandler` implementation type. |
| `AllowedMethods` | `IEnumerable<HttpMethod>` | HTTP methods the endpoint accepts. `null` or empty means all methods are accepted. |
| `Executor` | `ICompiledEndpointExecutor` | Pre-compiled delegate used by the middleware to dispatch requests. Set automatically during registration; throws `InvalidOperationException` if set more than once. |

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

If you want to serve the built-in OpenAPI document, call `UseEndpointHostOpenApi` **before** `UseEndpointHostBuilder`:

```csharp
app.UseEndpointHostOpenApi();   // must come first
app.UseEndpointHostBuilder();
```

---

## Configuring options (EndpointHostOptions)

`RegisterEndpointHostBuilder` accepts an optional `Action<EndpointHostOptions>` delegate that lets you tune two behaviours.

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `PathComparison` | `StringComparison` | `OrdinalIgnoreCase` | Controls whether path matching is case-sensitive. |
| `PassThroughOnNoExecutor` | `bool` | `true` | When `true`, a matched endpoint whose executor was never compiled passes the request to the next middleware. When `false`, the middleware returns HTTP 500 instead. |

```csharp
services.RegisterEndpointHostBuilder(options =>
{
    // Enforce exact-case path matching
    options.PathComparison = StringComparison.Ordinal;

    // Return 500 instead of falling through when executor is missing
    options.PassThroughOnNoExecutor = false;
});
```

Namespace: `RzR.Infrastructure.EndpointHosting.Configuration`.

---

## Implementing a handler

Create a class implementing `IEndpointHostRequestHandler`. This is where request validation and processing logic lives.

```csharp
using RzR.Infrastructure.EndpointHosting.Abstractions;

public class SysTimeEndpointHandler : IEndpointHostRequestHandler
{
    public Task<IEndpointHostResult> RequestProcessAsync(
        HttpContext context, CancellationToken cancellationToken = default)
    {
        IEndpointHostResult result = HttpMethods.IsGet(context.Request.Method)
            ? new SysTimeEndpointResult()
            : new StatusCodeResult(HttpStatusCode.MethodNotAllowed);

        return Task.FromResult(result);
    }

    public IEndpointHostResult RequestProcess(HttpContext context)
    {
        return HttpMethods.IsGet(context.Request.Method)
            ? new SysTimeEndpointResult()
            : new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
    }
}
```

---

## Implementing a result

Create a class implementing `IEndpointHostResult`. This is responsible for writing the HTTP response.

```csharp
using RzR.Infrastructure.EndpointHosting.Abstractions;

public class SysTimeEndpointResult : IEndpointHostResult
{
    public Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        Execute(context);
        return Task.CompletedTask;
    }

    public void Execute(HttpContext context)
        => context.Response.WriteAsync($"{DateTime.Now}");
}
```

For common response shapes, use the built-in factory instead of writing a custom result class. See [Built-in result types](#built-in-result-types).

---

## Registering endpoints

### Option 1 — Manual registration

Useful when you need full control over the endpoint name, path, active state, HTTP method constraints, or handler service lifetime.

```csharp
services.RegisterEndpointHostBuilder();

// Basic — responds to all HTTP methods, handler registered as Transient
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime");

// With active flag
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime", isActive: true);

// With HTTP method constraints
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime",
    new[] { HttpMethod.Get });

// With active flag, method constraints, and handler lifetime
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime",
    isActive: true, new[] { HttpMethod.Get, HttpMethod.Post },
    ServiceLifetime.Scoped);

// With a pre-configured Endpoint instance
services.AddHostEndpoint<SysTimeEndpointHandler>(new Endpoint(
    "sysTime", "/systime", typeof(SysTimeEndpointHandler), isActive: true));
```

### Option 2 — Attribute-driven registration (auto-discovery)

Decorate your handler with `[EndpointHost]` and call `AddHostEndpointsFromAssembly`. The library scans the assembly for all decorated handlers and registers them automatically.

The attribute signature is:

```csharp
[EndpointHost(string name, string path, bool isActive = true, params string[] httpMethods)]
```

HTTP methods are plain string names such as `"GET"` and `"POST"`.

```csharp
// 1. Decorate the handler
[EndpointHost("sysTime2", "/systime2", true, "GET")]
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

## Handler service lifetime

Every `AddHostEndpoint<T>` overload accepts an optional `ServiceLifetime` parameter that controls how the handler is registered in the DI container. The default is `Transient`.

```csharp
// Singleton handler — reused across all requests
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime",
    ServiceLifetime.Singleton);

// Scoped handler — one instance per request
services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime",
    ServiceLifetime.Scoped);
```

`AddHostEndpointsFromAssembly` exposes an equivalent overload:

```csharp
services.AddHostEndpointsFromAssembly(
    Assembly.GetExecutingAssembly(),
    ServiceLifetime.Scoped);
```

Endpoint metadata (the `Endpoint` object itself) is always registered as a singleton regardless of the handler lifetime you choose.

---

## Route parameters

Paths support named parameters in curly-brace syntax, for example `/users/{id}` or `/orders/{orderId}/items/{itemId}`. The segment count of the incoming request must match the template exactly — `/users/42/extra` will not match `/users/{id}`.

**Matching precedence** (highest to lowest):

1. Literal exact match — `/users/me` beats `/users/{id}`.
2. Among templates, the one with the highest `LiteralCount` (number of fixed segments) wins.
3. Registration order breaks remaining ties.

**Constraints and restrictions:**

- Parameter names must not be empty — an empty `{}` throws at registration time.
- Duplicate parameter names in the same template throw at registration time.
- Catch-all syntax (`{*rest}`) is not supported and is treated as a literal segment name.

**Reading captured values in a handler:**

Route values are stored on `HttpContext.Items` and accessed through extension methods in namespace `RzR.Infrastructure.EndpointHosting.Routing`.

```csharp
using RzR.Infrastructure.EndpointHosting.Routing;

public class UserEndpointHandler : IEndpointHostRequestHandler
{
    public async Task<IEndpointHostResult> RequestProcessAsync(
        HttpContext context, CancellationToken cancellationToken = default)
    {
        string rawId = context.GetEndpointRouteValue("id");
        if (rawId == null)
            return EndpointResults.NotFound();

        Guid userId;
        if (!Guid.TryParse(rawId, out userId))
            return EndpointResults.StatusCode(400);

        // ... fetch and return user
        return EndpointResults.Ok(new { Id = userId });
    }

    public IEndpointHostResult RequestProcess(HttpContext context)
        => throw new NotImplementedException();
}
```

`GetEndpointRouteValues()` returns an `IReadOnlyDictionary<string, string>` containing all captured segments. `GetEndpointRouteValue(name)` returns a single value or `null` when the name was not captured. All values are strings — parse to `int`, `Guid`, or another type in your handler.

---

## Built-in result types

The `EndpointResults` factory (namespace `RzR.Infrastructure.EndpointHosting.Results`) provides ready-made `IEndpointHostResult` implementations for the most common HTTP responses. Use these instead of authoring a custom result class.

| Method | Status | Content-Type | Notes |
| --- | --- | --- | --- |
| `Ok()` | 200 | — | Empty body. |
| `Ok<T>(payload)` | 200 | `application/json; charset=utf-8` | Serializes `payload` by declared type `T`. |
| `Json<T>(payload, statusCode = 200)` | configurable | `application/json; charset=utf-8` | Serializes `payload` by declared type `T`. |
| `Text(text, statusCode = 200, contentType = null)` | configurable | `text/plain; charset=utf-8` | `contentType` overrides the default. |
| `NoContent()` | 204 | — | Empty body. |
| `NotFound()` | 404 | — | Empty body. |
| `StatusCode(code)` | configurable | — | Empty body. |
| `Problem(title, detail = null, statusCode = 500, type = null, instance = null)` | configurable | `application/problem+json` | RFC 7807 problem details. |

> **JSON serialization note:** `Ok<T>` and `Json<T>` serialize by the declared type `T`. If you want the runtime type to drive serialization, declare the payload as `object`.

```csharp
using RzR.Infrastructure.EndpointHosting.Results;

public class ProductHandler : IEndpointHostRequestHandler
{
    public async Task<IEndpointHostResult> RequestProcessAsync(
        HttpContext context, CancellationToken cancellationToken = default)
    {
        var product = new { Id = 1, Name = "Widget" };

        // 200 OK with JSON body
        return EndpointResults.Ok(product);
    }

    public IEndpointHostResult RequestProcess(HttpContext context)
        => throw new NotImplementedException();
}
```

Other examples:

```csharp
// Plain text response
return EndpointResults.Text("Hello, world!");

// Custom status code
return EndpointResults.StatusCode(202);

// Problem details
return EndpointResults.Problem(
    title: "Validation failed",
    detail: "The 'name' field is required.",
    statusCode: 422);
```

---

## OpenAPI document

The library can serve a self-contained OpenAPI 3.0.1 JSON document without any third-party generator.

**Registration:**

```csharp
using RzR.Infrastructure.EndpointHosting.OpenApi;

app.UseEndpointHostOpenApi(
    path: "/openapi.json",    // default
    title: "My API",
    version: "1.0.0");        // default

app.UseEndpointHostBuilder(); // must come after UseEndpointHostOpenApi
```

`UseEndpointHostOpenApi` must be called **before** `UseEndpointHostBuilder`. If the router has not been registered via `RegisterEndpointHostBuilder`, the endpoint returns HTTP 500.

The document is rebuilt on every request from the current endpoint registry, so newly activated endpoints are reflected immediately without restarting the application.

> **Known limitation:** Endpoints registered without an HTTP method constraint are emitted in the OpenAPI document as GET-only operations.

---

## Observability

The library emits telemetry through a named `ActivitySource` (all TFMs) and named metrics counters (.NET 6 and later).

Both constants live in `RzR.Infrastructure.EndpointHosting.Diagnostics.EndpointHostDiagnostics`:

```csharp
EndpointHostDiagnostics.ActivitySourceName  // "RzR.Infrastructure.EndpointHosting"
EndpointHostDiagnostics.MeterName           // "RzR.Infrastructure.EndpointHosting"
```

**Activity tags** set per dispatched request:

| Tag | Value |
|---|---|
| `endpoint.name` | The registered endpoint name. |
| `http.method` | The HTTP method of the request. |
| `http.route` | The matched route path. |
| `endpoint.matched` | `true` / `false`. |

**Metrics counters** (NET6+ only):

| Counter | Description |
|---|---|
| `endpoint_host.requests.matched` | Requests matched to a registered endpoint. |
| `endpoint_host.requests.not_matched` | Requests that did not match any endpoint. |
| `endpoint_host.requests.no_executor` | Matched endpoints whose executor was not compiled. |

**OpenTelemetry setup:**

```csharp
using RzR.Infrastructure.EndpointHosting.Diagnostics;

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(EndpointHostDiagnostics.ActivitySourceName))
    .WithMetrics(metrics => metrics
        .AddMeter(EndpointHostDiagnostics.MeterName));
```

Tracing works on all supported TFMs. Metrics are only emitted on .NET 6 and later; the meter is not created on older targets.

---

## Trimming and AOT

The library is intentionally **not** marked `IsTrimmable` or `IsAotCompatible`. It uses LINQ expression-tree compilation to build per-handler delegates, and `AddHostEndpointsFromAssembly` calls `Assembly.GetTypes()` at startup — both are incompatible with static linking.

The following members carry `[RequiresUnreferencedCode]` (NET5+) and `[RequiresDynamicCode]` (NET7+) annotations:

- `CompiledEndpointExecutorFactory.Create` / `CreateTask`
- `AddHostEndpointsFromAssembly`

In publish scenarios that enable trimming or Native AOT, register each handler explicitly with `AddHostEndpoint<T>()` instead of using the assembly scanner.

```csharp
// Safe in trimmed / AOT builds
services.RegisterEndpointHostBuilder();
services.AddHostEndpoint<UserEndpointHandler>("users", "/users/{id}");
services.AddHostEndpoint<ProductHandler>("products", "/products");
```

---

## Startup code example

```csharp
using RzR.Infrastructure.EndpointHosting.Discovery;
using RzR.Infrastructure.EndpointHosting.OpenApi;

public void ConfigureServices(IServiceCollection services)
{
    services.RegisterEndpointHostBuilder(options =>
    {
        options.PathComparison = StringComparison.OrdinalIgnoreCase;
        options.PassThroughOnNoExecutor = true;
    });

    // Auto-discover handlers decorated with [EndpointHost]
    services.AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly());

    // Optionally register additional endpoints manually
    services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime");

    services.AddControllers();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRouting();

    // OpenAPI must be registered before the host middleware
    app.UseEndpointHostOpenApi("/openapi.json", "My API", "1.0.0");
    app.UseEndpointHostBuilder();

    app.UseEndpoints(endpoints => endpoints.MapControllers());
}
```
