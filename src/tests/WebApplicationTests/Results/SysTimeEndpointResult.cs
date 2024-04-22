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
using System.Threading.Tasks;

namespace WebApplicationTests.Results
{
    public class SysTimeEndpointResult : IEndpointHostResult
    {
        /// <inheritdoc />
        public async Task ExecuteAsync(HttpContext context)
            => await Task.Run(() => Execute(context));

        /// <inheritdoc />
        public void Execute(HttpContext context) => context.Response.WriteAsync($"{DateTime.Now}");
    }
}