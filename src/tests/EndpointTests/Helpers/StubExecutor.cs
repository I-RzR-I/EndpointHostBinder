// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 23:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:07
//  ***********************************************************************
//  <copyright file="StubExecutor.cs" company="RzR SOFT & TECH">
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
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A no-op test double for ICompiledEndpointExecutor used in SetOnce executor tests.
    /// </summary>
    /// =================================================================================================
    internal sealed class StubExecutor : ICompiledEndpointExecutor
    {
        /// <inheritdoc />
        public Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <inheritdoc />
        public void Execute(HttpContext context) { }
    }
}