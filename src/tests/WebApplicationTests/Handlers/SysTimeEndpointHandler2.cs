// ***********************************************************************
//  Assembly         : RzR.Shared.Services.WebApplicationTests
//  Author           : RzR
//  Created On       : 2024-04-21 21:16
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 21:16
// ***********************************************************************
//  <copyright file="SysTimeEndpointHandler.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

using DomainCommonExtensions.DataTypeExtensions;
using EndpointHostBinder.Abstractions;
using EndpointHostBinder.Attributes;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebApplicationTests.Results;

namespace WebApplicationTests.Handlers
{
    [EndpointHost("sysTime2", "/systime2", true, new[] { "GET" })]
    public class SysTimeEndpointHandler2 : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public Task<IEndpointHostResult> RequestProcessAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            IEndpointHostResult result = HttpMethods.IsGet(context.Request.Method).IsFalse()
                ? new StatusCodeResult(HttpStatusCode.MethodNotAllowed)
                : new SysTimeEndpointResult2();

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
        {
            IEndpointHostResult result;
            result = HttpMethods.IsGet(context.Request.Method).IsFalse()
                ? new StatusCodeResult(HttpStatusCode.MethodNotAllowed)
                : new SysTimeEndpointResult2();

            return result;
        }
    }
}