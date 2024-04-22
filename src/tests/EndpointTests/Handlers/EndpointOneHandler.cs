// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2024-04-19 22:23
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-19 22:23
// ***********************************************************************
//  <copyright file="EndpointOneHandler.cs" company="">
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
    public class EndpointOneHandler : IEndpointHostRequestHandler
    {
        /// <inheritdoc />
        public async Task<IEndpointHostResult> RequestProcessAsync(HttpContext context)
            => throw new System.NotImplementedException();

        /// <inheritdoc />
        public IEndpointHostResult RequestProcess(HttpContext context)
            => throw new System.NotImplementedException();
    }
}