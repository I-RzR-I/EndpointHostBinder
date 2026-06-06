// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 23:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:05
//  ***********************************************************************
//  <copyright file="RouteCapturingHandler.cs" company="RzR SOFT & TECH">
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
using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests.Handlers
{
    internal class RouteCapturingHandler : IEndpointHostRequestHandler
    {
        private readonly Action<HttpContext> _onInvoke;

        public RouteCapturingHandler(Action<HttpContext> onInvoke) => _onInvoke = onInvoke;

        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(
            HttpContext context, CancellationToken cancellationToken = default)
        {
            _onInvoke(context);

            return Task.FromResult<IEndpointHostResult>(new FunctionalEndpointResult());
        }

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
        {
            _onInvoke(context);

            return new FunctionalEndpointResult();
        }
    }
}