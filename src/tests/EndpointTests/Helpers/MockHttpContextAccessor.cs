// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 23:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:07
//  ***********************************************************************
//  <copyright file="MockHttpContextAccessor.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace EndpointTests.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A mock HTTP context accessor.
    /// </summary>
    /// <seealso cref="T:Microsoft.AspNetCore.Http.IHttpContextAccessor" />
    /// =================================================================================================
    public class MockHttpContextAccessor : IHttpContextAccessor
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="MockHttpContextAccessor" /> class.
        /// </summary>
        /// =================================================================================================
        public MockHttpContextAccessor()
        {
            var services = new ServiceCollection();
            HttpContext.RequestServices = services.BuildServiceProvider();
        }

        /// <inheritdoc />
        public HttpContext HttpContext { get; set; } = new DefaultHttpContext();
    }
}