> **Note** This repository is developed using .netstandard2.0, .netstandard2.1.

| Name     | Details |
|----------|----------|
| EndpointHostBinder | [![NuGet Version](https://img.shields.io/nuget/v/EndpointHostBinder.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/EndpointHostBinder/) [![Nuget Downloads](https://img.shields.io/nuget/dt/EndpointHostBinder.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/EndpointHostBinder)|

A middleware that listens to application requests and validates them, allows them to be processed by `IEndpointHostRequestHandler` and `IEndpointHostResult`. Exposes application endpoints without using controllers.

Key features:
- Route incoming HTTP requests to lightweight handler classes — no controllers required.
- Register endpoints **manually** via `AddHostEndpoint<T>()` with full control over name, path, active state, and HTTP method constraints.
- Register endpoints **automatically** via `AddHostEndpointsFromAssembly()` by decorating handlers with `[EndpointHost]`.
- Mix both approaches safely — duplicate registrations are detected and skipped.
- Pre-compiled LINQ expression tree executors eliminate per-request reflection overhead.

To understand more efficiently how you can use available functionalities please consult the [usage documentation](docs/usage.md).

**In case you wish to use it in your project, u can install the package from <a href="https://www.nuget.org/packages/EndpointHostBinder" target="_blank">nuget.org</a>** or specify what version you want:

> `Install-Package EndpointHostBinder -Version x.x.x.x`

## Content
1. [USING](docs/usage.md)
1. [CHANGELOG](docs/CHANGELOG.md)
1. [BRANCH-GUIDE](docs/branch-guide.md)