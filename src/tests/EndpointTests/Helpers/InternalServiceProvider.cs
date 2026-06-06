// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 23:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:06
//  ***********************************************************************
//  <copyright file="InternalServiceProvider.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using EndpointTests.Handlers;
using System;

#endregion

namespace EndpointTests.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A minimal IServiceProvider test double that resolves the handler types used in
    ///     EndpointHostRouterTests.
    /// </summary>
    /// =================================================================================================
    internal class InternalServiceProvider : IServiceProvider
    {
        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(EndpointOneHandler))
                return new EndpointOneHandler();
            if (serviceType == typeof(EndpointTwoHandler))
                return new EndpointTwoHandler();
            if (serviceType == typeof(EndpointThreeHandler))
                return new EndpointThreeHandler();
            if (serviceType == typeof(FunctionalEndpointHandler))
                return new FunctionalEndpointHandler();

            throw new InvalidOperationException($"No registration for {serviceType.Name}");
        }
    }
}