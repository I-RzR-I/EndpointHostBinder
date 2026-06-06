// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 20:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:01
//  ***********************************************************************
//  <copyright file="OkResult.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Http;
using RzR.Extensions.Domain.Validation;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Results
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An <see cref="IEndpointHostResult" /> that produces an HTTP 200 OK response with an empty
    ///     body. Use <see cref="OkResult{T}" /> to include a JSON payload.
    /// </summary>
    /// <seealso cref="T:RzR.Infrastructure.EndpointHosting.Abstractions.IEndpointHostResult" />
    /// =================================================================================================
    public sealed class OkResult : IEndpointHostResult
    {
        /// <inheritdoc />
        public Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = StatusCodes.Status200OK;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Execute(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = StatusCodes.Status200OK;
        }
    }
}