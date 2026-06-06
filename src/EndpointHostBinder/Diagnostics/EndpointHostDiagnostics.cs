// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:18
//  ***********************************************************************
//  <copyright file="EndpointHostDiagnostics.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Diagnostics;

#if NET6_0_OR_GREATER
using System.Diagnostics.Metrics;
#endif

#endregion

namespace RzR.Infrastructure.EndpointHosting.Diagnostics
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Central holder for the library's tracing and (on .NET 6+) metrics instances.
    ///     Consumers can subscribe using <see cref="MeterName" /> and
    ///     <see cref="ActivitySourceName" /> as the listener filter.
    ///     The Meter and Counter fields are only available on .NET 6 and later;
    ///     on earlier runtimes only <see cref="ActivitySource" /> is present.
    /// </summary>
    /// =================================================================================================
    public static class EndpointHostDiagnostics
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The name used for the Meter (NET 6+) and to subscribe via MeterListener.
        /// </summary>
        /// =================================================================================================
        public const string MeterName = "RzR.Infrastructure.EndpointHosting";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The name used for the <see cref="System.Diagnostics.ActivitySource" /> and to subscribe
        ///     via <see cref="ActivityListener" />.
        /// </summary>
        /// =================================================================================================
        public const string ActivitySourceName = "RzR.Infrastructure.EndpointHosting";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) activity source for distributed tracing of endpoint dispatch.
        ///     Available on all supported TFMs (netstandard2.0+, net5.0+).
        /// </summary>
        /// =================================================================================================
        internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);

#if NET6_0_OR_GREATER
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) meter instance for the library. Dispose is intentionally not called;
        ///     the instance lives for the process lifetime.
        ///     Only present on .NET 6 and later — System.Diagnostics.Metrics is a net6+ BCL API.
        /// </summary>
        /// =================================================================================================
        internal static readonly Meter Meter = new Meter(MeterName);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Counter incremented for every request that successfully matched an active endpoint.
        /// </summary>
        /// =================================================================================================
        internal static readonly Counter<long> RequestsMatched =
            Meter.CreateCounter<long>("endpoint_host.requests.matched",
                description: "Number of requests that matched an active registered endpoint.");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Counter incremented for every request where no matching endpoint was found.
        /// </summary>
        /// =================================================================================================
        internal static readonly Counter<long> RequestsNotMatched =
            Meter.CreateCounter<long>("endpoint_host.requests.not_matched",
                description: "Number of requests that did not match any active registered endpoint.");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Counter incremented when an endpoint is matched but its compiled executor is null.
        /// </summary>
        /// =================================================================================================
        internal static readonly Counter<long> RequestsNoExecutor =
            Meter.CreateCounter<long>("endpoint_host.requests.no_executor",
                description: "Number of requests where a matched endpoint had no compiled executor.");
#endif
    }
}