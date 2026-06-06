// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:07
//  ***********************************************************************
//  <copyright file="OkJsonPayload.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

namespace EndpointTests.Results
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A simple data carrier used by OkJsonHandler in integration tests.
    /// </summary>
    /// =================================================================================================
    internal sealed class OkJsonPayload
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the name field.
        /// </summary>
        /// =================================================================================================
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the age field.
        /// </summary>
        /// =================================================================================================
        public int Age { get; set; }
    }
}