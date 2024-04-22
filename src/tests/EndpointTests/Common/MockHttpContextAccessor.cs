// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2024-04-19 22:02
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-19 22:05
// ***********************************************************************
//  <copyright file="MockHttpContextAccessor.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace EndpointTests.Common
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A mock HTTP context accessor.
    /// </summary>
    /// <seealso cref="T:Microsoft.AspNetCore.Http.IHttpContextAccessor"/>
    /// =================================================================================================
    public class MockHttpContextAccessor : IHttpContextAccessor
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="MockHttpContextAccessor"/> class.
        /// </summary>
        /// =================================================================================================
        public MockHttpContextAccessor()
        {
            var services = new ServiceCollection();
            HttpContext.RequestServices = services.BuildServiceProvider();
        }

        /// <inheritdoc/>
        public HttpContext HttpContext { get; set; } = new DefaultHttpContext();
    }
}