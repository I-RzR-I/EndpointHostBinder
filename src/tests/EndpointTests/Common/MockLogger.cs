// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2024-04-19 22:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-19 22:05
// ***********************************************************************
//  <copyright file="MockLogger.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.Extensions.Logging;

#endregion

namespace EndpointTests.Common
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