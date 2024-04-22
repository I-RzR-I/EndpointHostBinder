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
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using WebApplicationTests.Results;

namespace WebApplicationTests.Handlers
{
    public class SysTimeEndpointHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public async Task<IEndpointHostResult> RequestProcessAsync(HttpContext context)
            => await Task.Run(() => RequestProcess(context));

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
        {
            IEndpointHostResult result;
            result = HttpMethods.IsGet(context.Request.Method).IsFalse() 
                ? new StatusCodeResult(HttpStatusCode.MethodNotAllowed) 
                : new SysTimeEndpointResult();

            return result;
        }
    }
}