### **v3.0.0.733** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 07-06-2026
* [DEV] (RzR) -> Renames the assembly, root namespace, and NuGet package from EndpointHostBinder to RzR.Infrastructure.EndpointHosting, expands the target matrix, and adds a large feature set on top of the existing controller-less endpoint middleware.
* [DEV] (RzR) -> Add EndpointHostOptions: configurable PathComparison (default OrdinalIgnoreCase) and PassThroughOnNoExecutor (default true), wired via RegisterEndpointHostBuilder(Action<EndpointHostOptions>).
* [DEV] (RzR) -> Route parameters: templated paths (e.g. /users/{id}) with deterministic specificity (literal > highest LiteralCount > registration order); captured values exposed through HttpContextRouteValuesExtensions.
* [DEV] (RzR) -> Self-contained OpenAPI 3.0.1 document generation via UseEndpointHostOpenApi(path, title, version) - no third-party generator.
* [DEV] (RzR) -> Built-in result types via the EndpointResults factory (Ok, Ok<T>, Json<T>, Text, NoContent, NotFound, StatusCode, Problem).
* [DEV] (RzR) -> Configurable handler service lifetime on AddHostEndpoint<T> and AddHostEndpointsFromAssembly (endpoint metadata stays singleton).

### **v2.0.0.7486** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 24-03-2026
* [DEV] (RzR) -> Added `[EndpointHost]` attribute for declarative, attribute-driven endpoint registration.
* [DEV] (RzR) -> Added `AddHostEndpointsFromAssembly(Assembly)` extension method for automatic discovery and registration of all `[EndpointHost]` - decorated handlers in a given assembly.
* [DEV] (RzR) -> Added duplicate registration 'guard' in `AddHostEndpointsFromAssembly` - handlers already registered manually are skipped automatically.
* [DEV] (RzR) -> Added `AllowedMethods` property to `Endpoint` model; routing now respects HTTP method constraints.
* [DEV] (RzR) -> Added new `AddHostEndpoint<T>` overloads: with `allowedMethods`, with `isActive + allowedMethods`, and with a pre-configured `Endpoint` instance.
* [DEV] (RzR) -> Added pre-compiled LINQ expression tree executor (`CompiledEndpointExecutor`/`CompiledEndpointExecutorFactory`) replacing per-request reflection for zero-overhead dispatch.
* [DEV] (RzR) -> Improved XML documentation across all public types and members.

* [BREAKING] (RzR) -> `IEndpointHostRequestHandler.RequestProcessAsync` now requires a `CancellationToken cancellationToken = default` parameter. Existing implementations must be updated.
* [BREAKING] (RzR) -> `IEndpointHostResult.ExecuteAsync` now requires a `CancellationToken cancellationToken = default` parameter. Existing implementations must be updated.

### **v1.0.2.8175**
-> Update the minimum related packages version.
