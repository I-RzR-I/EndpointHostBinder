// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:28
//  ***********************************************************************
//  <copyright file="Log.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.Extensions.Logging;
using System;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Diagnostics
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Logging delegates compiled via
    ///     <see cref="LoggerMessage.Define" /> for the endpoint hosting infrastructure.
    ///     Each delegate has a stable <see cref="EventId" /> so consumers can filter by id.
    /// </summary>
    /// =================================================================================================
    internal static class Log
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs that an endpoint was matched and its handler is about to be executed.
        ///     Parameters: endpoint name, endpoint path.
        /// </summary>
        /// =================================================================================================
        internal static readonly Action<ILogger, string, string, Exception> ExecutingEndpoint =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(1000, "ExecutingEndpoint"),
                "Executing endpoint [{name}] for path [{path}]");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs that an endpoint was found by the router (literal match).
        ///     Parameters: endpoint name, handler full type name.
        /// </summary>
        /// =================================================================================================
        internal static readonly Action<ILogger, string, string, Exception> EndpointFoundLiteral =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(1001, "EndpointFoundLiteral"),
                "Endpoint enabled: [{endpoint}], successfully find handler: [{endpointHandler}]");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs that an endpoint was found by the router (template match).
        ///     Parameters: endpoint name, handler full type name.
        /// </summary>
        /// =================================================================================================
        internal static readonly Action<ILogger, string, string, Exception> EndpointFoundTemplate =
            LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(1002, "EndpointFoundTemplate"),
                "Endpoint enabled: [{endpoint}], successfully find handler (template): [{endpointHandler}]");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs that the request path did not match any active endpoint.
        ///     Parameters: request path.
        /// </summary>
        /// =================================================================================================
        internal static readonly Action<ILogger, string, Exception> NoMatchFound =
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(1003, "NoMatchFound"),
                "Request path [{path}] no match any active endpoints");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs that a matched endpoint has no compiled executor assigned.
        ///     Parameters: request path.
        /// </summary>
        /// =================================================================================================
        internal static readonly Action<ILogger, string, Exception> NoExecutor =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                new EventId(1004, "NoExecutor"),
                "Endpoint [{path}] has no compiled executor");

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Logs an unhandled exception that propagated out of the endpoint executor.
        ///     Parameters: exception message.
        /// </summary>
        /// =================================================================================================
        internal static readonly Action<ILogger, string, Exception> UnhandledException =
            LoggerMessage.Define<string>(
                LogLevel.Critical,
                new EventId(1005, "UnhandledException"),
                "Unhandled exception: {exception}");
    }
}