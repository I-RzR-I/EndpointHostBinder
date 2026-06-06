> **Note** This repository targets seven frameworks: netstandard2.0, netstandard2.1, net5.0, net6.0, net7.0, net8.0, net9.0.

| Name     | Details |
|----------|----------|
| RzR.Infrastructure.EndpointHosting | [![NuGet Version](https://img.shields.io/nuget/v/RzR.Infrastructure.EndpointHosting.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Infrastructure.EndpointHosting/) [![Nuget Downloads](https://img.shields.io/nuget/dt/RzR.Infrastructure.EndpointHosting.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Infrastructure.EndpointHosting)|


<details>

  <summary>Old version</summary>
  
[![NuGet Version](https://img.shields.io/nuget/v/EndpointHostBinder.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/EndpointHostBinder/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/EndpointHostBinder.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/EndpointHostBinder)

</details>

<br />

A middleware that listens to application requests and validates them, allows them to be processed by `IEndpointHostRequestHandler` and `IEndpointHostResult`. Exposes application endpoints without using controllers.

Key features:
- Route incoming HTTP requests to lightweight handler classes — no controllers required.
- Register endpoints **manually** via `AddHostEndpoint<T>()` with full control over name, path, active state, and HTTP method constraints.
- Register endpoints **automatically** via `AddHostEndpointsFromAssembly()` by decorating handlers with `[EndpointHost]`.
- Mix both approaches safely — duplicate registrations are detected and skipped.
- Pre-compiled LINQ expression tree executors eliminate per-request reflection overhead.
- Configurable options: control path match case-sensitivity and behaviour when a matched endpoint has no executor.
- Templated route paths with named parameters (e.g. `/users/{id}`) and a deterministic specificity precedence rule.
- Self-contained OpenAPI 3.0.1 document generation served from a configurable path — no third-party generator required.
- Built-in result factory (`EndpointResults`) covering the most common HTTP responses: JSON, plain text, problem details, 204, 404, and arbitrary status codes.
- Observability via .NET `ActivitySource` (all TFMs) and named metrics counters (.NET 6+).

To understand more efficiently how you can use available functionalities please consult the [usage documentation](docs/usage.md).

**In case you wish to use it in your project, u can install the package from <a href="https://www.nuget.org/packages/RzR.Infrastructure.EndpointHosting" target="_blank">nuget.org</a>** or specify what version you want:

> `Install-Package RzR.Infrastructure.EndpointHosting -Version x.x.x.x`

## Content
1. [USING](docs/usage.md)
1. [CHANGELOG](docs/CHANGELOG.md)
1. [BRANCH-GUIDE](docs/branch-guide.md)
