using Microsoft.AspNetCore.Http;
using RzR.Extensions.Domain.Primitives;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Attributes;
using RzR.Infrastructure.EndpointHosting.Results;
using System.Threading;
using System.Threading.Tasks;
using WebApplicationOpenApiTests.Models;

namespace WebApplicationOpenApiTests.Handlers
{
    /// <summary>
    /// Returns all users as a JSON array.
    /// Discovered by assembly scan via [EndpointHost].
    /// </summary>
    [EndpointHost("getUsers", "/users", true, "GET")]
    public class GetUsersHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            IEndpointHostResult result = HttpMethods.IsGet(context.Request.Method).IsFalse()
                ? EndpointResults.StatusCode(405)
                : EndpointResults.Ok(SeedData.Users);

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
        {
            IEndpointHostResult result = HttpMethods.IsGet(context.Request.Method).IsFalse()
                ? EndpointResults.StatusCode(405)
                : EndpointResults.Ok(SeedData.Users);

            return result;
        }
    }
}
