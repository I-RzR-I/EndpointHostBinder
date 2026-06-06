// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 20:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:17
//  ***********************************************************************
//  <copyright file="EndpointHostOptions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Configuration
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Configuration options that control the behaviour of the endpoint host infrastructure,
    ///     including path-matching semantics and fall-through handling.
    /// </summary>
    /// =================================================================================================
    public class EndpointHostOptions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the <see cref="StringComparison" /> used when matching an incoming request
        ///     path against registered endpoint paths.
        /// </summary>
        /// <remarks>
        ///     Defaults to <see cref="StringComparison.OrdinalIgnoreCase" />, which preserves the
        ///     original case-insensitive behaviour. Change to <see cref="StringComparison.Ordinal" />
        ///     for strict case-sensitive path matching.
        /// </remarks>
        /// <value>
        ///     The <see cref="StringComparison" /> strategy for path matching.
        /// </value>
        /// =================================================================================================
        public StringComparison PathComparison { get; set; } = StringComparison.OrdinalIgnoreCase;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets a value indicating whether a request that matches an endpoint path but whose
        ///     endpoint has no compiled executor should be forwarded to the next middleware
        ///      (pass-through) rather than being short-circuited.
        /// </summary>
        /// <remarks>
        ///     Defaults to <see langword="true" />, which preserves the original behaviour where an
        ///     endpoint without an executor falls through to the next middleware in the pipeline. Set to
        ///     <see langword="false" /> to short-circuit the pipeline and return an empty 500 response
        ///     instead, which is appropriate when a missing executor always indicates a configuration
        ///     error.
        /// </remarks>
        /// <value>
        ///     <see langword="true" /> to call the next middleware when no executor is found;
        ///     <see langword="false" /> to short-circuit.
        /// </value>
        /// =================================================================================================
        public bool PassThroughOnNoExecutor { get; set; } = true;
    }
}