using Microsoft.AspNetCore.Builder;
using RzR.Infrastructure.EndpointHosting.Discovery;
using RzR.Infrastructure.EndpointHosting.OpenApi;
using System.Net.Http;
using System.Reflection;
using WebApplicationOpenApiTests.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterEndpointHostBuilder(options => { })
    .AddHostEndpoint<SysTimeHandler>("sysTime", "/systime")
    .AddHostEndpoint<GetUserByIdHandler>("getUserById", "/users/{id}", true,
        new[] { HttpMethod.Get })
    .AddHostEndpoint<CreateUserHandler>("createUser", "/users", true,
        new[] { HttpMethod.Post });

builder.Services.AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Serves the library-generated OpenAPI document at /openapi.json.
// Must be registered BEFORE UseEndpointHostBuilder so the path is
// intercepted by the OpenApi middleware branch first.
app.UseEndpointHostOpenApi("/openapi.json", "EndpointHostBinder Demo API", "1.0.0");

// Renders the EndpointHostBinder-generated /openapi.json through
// Swagger UI at /swagger. AddSwaggerGen / UseSwagger are intentionally
// NOT called — this project showcases the library's own document only.
app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi.json", "EndpointHostBinder Demo API v1"));

app.UseEndpointHostBuilder();

app.Run();
