An endpoint is defined by a few properties: `Name`, `Path`, `IsActive`, and `EndpointType`.

- `Name` -> Name of the endpoint;
- `Path` -> Path to the endpoint/resource which can be invoked;
- `IsActive` -> Is active or not endpoint;
- `EndpointType` -> The type of the current endpoint.

After intallation in the `Startup.cs` class add the following blocks:
```csharp
public void ConfigureServices(IServiceCollection services)
{
           ...
           //   To inject routes processor and validator
           services.RegisterEndpointHostBuilder();
           
           ...
}
```
 and 
 ```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
            ...
            // To activate middleware
            app.UseEndpointHostBuilder();
            
            ...
}
```

To expose your endpoint create an implementation of `IEndpointHostRequestHandler` -> allow you to process the current request and add your request validation.

After this operation add the endpoint processor, an implementation of `IEndpointHostResult` where are all functionalities on this endpoint.

A simple example of the implementation:
```csharp
public class SysTimeEndpointHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public async Task<IEndpointHostResult> RequestProcessAsync(HttpContext context)
            => await Task.Run(() => RequestProcess(context));

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
        {
            IEndpointHostResult result;
            result = HttpMethods.IsGet(context.Request.Method).IsFalse() 
                ? new StatusCodeResult(HttpStatusCode.MethodNotAllowed) 
                : new SysTimeEndpointResult();

            return result;
        }
    }
```

```csharp
public class SysTimeEndpointResult : IEndpointHostResult
    {
        /// <inheritdoc />
        public async Task ExecuteAsync(HttpContext context)
            => await Task.Run(() => Execute(context));

        /// <inheritdoc />
        public void Execute(HttpContext context) => context.Response.WriteAsync($"{DateTime.Now}");
    }
```

To be able to invoke current endpoint, also in the `Startup.cs` you must to register it.
```csharp
public void ConfigureServices(IServiceCollection services)
{
           ...
           //   To inject routes processor and validator
           services.RegisterEndpointHostBuilder();
           
           //   Register previously created endpoint
            services.AddHostEndpoint<SysTimeEndpointHandler>("sysTime", "/systime");
           
           ...
}
```

