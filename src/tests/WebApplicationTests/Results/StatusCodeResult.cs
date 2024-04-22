// ***********************************************************************
//  Assembly         : RzR.Shared.Services.WebApplicationTests
//  Author           : RzR
//  Created On       : 2024-04-21 21:24
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 21:24
// ***********************************************************************
//  <copyright file="StatusCodeResult.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

using EndpointHostBinder.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace WebApplicationTests.Results
{
    public class StatusCodeResult : IEndpointHostResult
    {
        public int StatusCode { get; }

        public StatusCodeResult(HttpStatusCode statusCode)
            => StatusCode = (int)statusCode;

        public StatusCodeResult(int statusCode)
            => StatusCode = statusCode;

        public Task ExecuteAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCode;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Execute(HttpContext context)
            => context.Response.StatusCode = StatusCode;
    }
}