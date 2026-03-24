// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2026-03-18 20:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-18 20:22
// ***********************************************************************
//  <copyright file="FunctionalEndpointResult.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using EndpointHostBinder.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests.Results
{
    public class FunctionalEndpointResult : IEndpointHostResult
    {
        public const string ResponseBody = "functional-ok";

        /// <inheritdoc />
        public Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            return context.Response.WriteAsync(ResponseBody, cancellationToken);
        }

        /// <inheritdoc />
        public void Execute(HttpContext context) => context.Response.StatusCode = (int)HttpStatusCode.OK;
    }
}