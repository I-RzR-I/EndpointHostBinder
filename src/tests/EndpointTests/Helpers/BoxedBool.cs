// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 23:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:06
//  ***********************************************************************
//  <copyright file="BoxedBool.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

namespace EndpointTests.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A mutable boolean wrapper used in middleware pipeline tests to capture whether a
    ///     delegate was invoked.
    /// </summary>
    /// =================================================================================================
    internal class BoxedBool
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets a value indicating whether the tracked event occurred.
        /// </summary>
        /// =================================================================================================
        public bool Value { get; set; }
    }
}