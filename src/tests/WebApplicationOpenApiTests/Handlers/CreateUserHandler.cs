using Microsoft.AspNetCore.Http;
using RzR.Extensions.Domain.Primitives;
using RzR.Extensions.Domain.Text;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Results;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplicationOpenApiTests.Handlers
{
    /// <summary>
    /// Simulates creating a user via POST /users.
    /// Returns 201 on valid input, or an RFC 7807 Problem for bad requests.
    /// </summary>
    public class CreateUserHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(HttpContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(RequestProcess(context));

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method).IsFalse())
                return EndpointResults.StatusCode(405);

            var contentType = context.Request.ContentType;

            if (contentType.IsMissing() || contentType.Contains("application/json").IsFalse())
                return EndpointResults.Problem(
                    title: "Unsupported Media Type",
                    detail: "Request body must be application/json.",
                    statusCode: 415);

            return EndpointResults.StatusCode(201);
        }
    }
}
