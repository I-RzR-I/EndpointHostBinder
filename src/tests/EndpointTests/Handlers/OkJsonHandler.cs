// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:04
//  ***********************************************************************
//  <copyright file="OkJsonHandler.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using EndpointTests.Results;
using Microsoft.AspNetCore.Http;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Results;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests.Handlers
{
    internal sealed class OkJsonHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(
            HttpContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(EndpointResults.Ok(new OkJsonPayload { Name = "Alice", Age = 30 }));

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
            => EndpointResults.Ok(new OkJsonPayload { Name = "Alice", Age = 30 });
    }
}