// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:05
//  ***********************************************************************
//  <copyright file="StatusCodeHandler.cs" company="RzR SOFT & TECH">
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
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests.Handlers
{
    internal sealed class StatusCodeHandler : IEndpointHostRequestHandler
    {
        /// <summary>The HTTP status code returned by this handler.</summary>
        public const int Code = 418;

        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(
            HttpContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(EndpointResults.StatusCode(Code));

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
            => EndpointResults.StatusCode(Code);
    }
}