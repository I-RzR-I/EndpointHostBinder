// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 23:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:07
//  ***********************************************************************
//  <copyright file="MockLogger.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.Extensions.Logging;

#endregion

namespace EndpointTests.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A mock logger.
    /// </summary>
    /// =================================================================================================
    public static class MockLogger
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Creates a new ILogger&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <returns>
        ///     An ILogger&lt;T&gt;
        /// </returns>
        /// =================================================================================================
        public static ILogger<T> Create<T>() => new LoggerFactory().CreateLogger<T>();
    }
}