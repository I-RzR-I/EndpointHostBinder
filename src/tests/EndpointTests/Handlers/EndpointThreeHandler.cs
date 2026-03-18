// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2026-03-18 20:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-18 20:19
// ***********************************************************************
//  <copyright file="EndpointThreeHandler.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using EndpointHostBinder.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests.Handlers
{
    public class EndpointThreeHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public async Task<IEndpointHostResult> RequestProcessAsync(
            HttpContext context,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
            => throw new NotImplementedException();
    }
}