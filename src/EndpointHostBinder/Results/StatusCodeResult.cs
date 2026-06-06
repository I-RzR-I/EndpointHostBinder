// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 20:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:01
//  ***********************************************************************
//  <copyright file="StatusCodeResult.cs" company="RzR SOFT & TECH">
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
    ///     An <see cref="IEndpointHostResult" /> that sets the HTTP response status code and writes
    ///     no body.
    /// </summary>
    /// <seealso cref="T:RzR.Infrastructure.EndpointHosting.Abstractions.IEndpointHostResult" />
    /// =================================================================================================
    public sealed class StatusCodeResult : IEndpointHostResult
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="StatusCodeResult" /> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to set on the response.</param>
        /// =================================================================================================
        public StatusCodeResult(int statusCode) => StatusCode = statusCode;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the HTTP status code that will be applied to the response.
        /// </summary>
        /// <value>
        ///     The HTTP status code (e.g. 200, 404, 500).
        /// </value>
        /// =================================================================================================
        public int StatusCode { get; }

        /// <inheritdoc />
        public Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = StatusCode;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Execute(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = StatusCode;
        }
    }
}