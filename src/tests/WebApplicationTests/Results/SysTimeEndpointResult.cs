// ***********************************************************************
//  Assembly         : RzR.Shared.Services.WebApplicationTests
//  Author           : RzR
//  Created On       : 2024-04-21 21:29
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 21:29
// ***********************************************************************
//  <copyright file="SysTimeEndpointResult.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

using EndpointHostBinder.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplicationTests.Results
{
    public class SysTimeEndpointResult : IEndpointHostResult
    {
        /// <inheritdoc />
        public Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            Execute(context);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Execute(HttpContext context) => context.Response.WriteAsync($"{DateTime.Now}");
    }
}