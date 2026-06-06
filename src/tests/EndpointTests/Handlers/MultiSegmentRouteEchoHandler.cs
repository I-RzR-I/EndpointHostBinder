// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:03
//  ***********************************************************************
//  <copyright file="MultiSegmentRouteEchoHandler.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Http;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Results;
using RzR.Infrastructure.EndpointHosting.Routing;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests.Handlers
{
    internal sealed class MultiSegmentRouteEchoHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(
            HttpContext context, CancellationToken cancellationToken = default)
        {
            var orderId = context.GetEndpointRouteValue("orderId") ?? string.Empty;
            var itemId = context.GetEndpointRouteValue("itemId") ?? string.Empty;

            return Task.FromResult(EndpointResults.Ok(new { orderId, itemId }));
        }

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
        {
            var orderId = context.GetEndpointRouteValue("orderId") ?? string.Empty;
            var itemId = context.GetEndpointRouteValue("itemId") ?? string.Empty;

            return EndpointResults.Ok(new { orderId, itemId });
        }
    }
}