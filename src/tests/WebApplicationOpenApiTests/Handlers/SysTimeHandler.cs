using Microsoft.AspNetCore.Http;
using RzR.Extensions.Domain.Primitives;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplicationOpenApiTests.Handlers
{
    /// <summary>
    /// Returns the current server date/time as plain text.
    /// Registered manually for GET /systime.
    /// </summary>
    public class SysTimeHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            IEndpointHostResult result = HttpMethods.IsGet(context.Request.Method).IsFalse()
                ? EndpointResults.StatusCode(405)
                : EndpointResults.Text(DateTime.UtcNow.ToString("O"));

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
        {
            IEndpointHostResult result = HttpMethods.IsGet(context.Request.Method).IsFalse()
                ? EndpointResults.StatusCode(405)
                : EndpointResults.Text(DateTime.UtcNow.ToString("O"));

            return result;
        }
    }
}
