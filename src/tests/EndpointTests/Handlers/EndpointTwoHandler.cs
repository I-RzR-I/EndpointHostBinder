// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2024-04-19 22:26
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-19 22:26
// ***********************************************************************
//  <copyright file="EndpointTwoHandler.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

using EndpointHostBinder.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EndpointTests.Handlers
{
    public class EndpointTwoHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public async Task<IEndpointHostResult> RequestProcessAsync(HttpContext context) 
            => throw new System.NotImplementedException();

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context) 
            => throw new System.NotImplementedException();
    }
}