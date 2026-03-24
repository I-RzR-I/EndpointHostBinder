// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2026-03-18 20:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-18 20:19
// ***********************************************************************
//  <copyright file="FunctionalEndpointHandler.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using EndpointHostBinder.Abstractions;
using EndpointTests.Results;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests.Handlers
{
    public class FunctionalEndpointHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(
            HttpContext context,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IEndpointHostResult>(new FunctionalEndpointResult());

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
            => new FunctionalEndpointResult();
    }
}