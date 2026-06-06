using Microsoft.AspNetCore.Http;
using RzR.Extensions.Domain.Primitives;
using RzR.Extensions.Domain.Text;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Results;
using RzR.Infrastructure.EndpointHosting.Routing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApplicationOpenApiTests.Models;

namespace WebApplicationOpenApiTests.Handlers
{
    /// <summary>
    /// Returns a single user matched by the route parameter {id}.
    /// Demonstrates route-value transport and multiple result types.
    /// </summary>
    public class GetUserByIdHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(HttpContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(RequestProcess(context));

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
        {
            if (HttpMethods.IsGet(context.Request.Method).IsFalse())
                return EndpointResults.StatusCode(405);

            var rawId = context.GetEndpointRouteValue("id");

            if (rawId.IsMissing())
                return EndpointResults.NotFound();

            if (!Guid.TryParse(rawId, out var id))
                return EndpointResults.StatusCode(400);

            var user = SeedData.Users.FirstOrDefault(u => u.Id == id);

            return user == null
                ? EndpointResults.NotFound()
                : EndpointResults.Ok(user);
        }
    }
}
