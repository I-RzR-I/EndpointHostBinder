// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2024-04-19 19:20
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-04 23:44
// ***********************************************************************
//  <copyright file="EndpointHostBinderMiddleware.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RzR.Extensions.Domain.Primitives;
using RzR.Extensions.Domain.Validation;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Configuration;
using RzR.Infrastructure.EndpointHosting.Diagnostics;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Host
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Core middleware that intercepts incoming HTTP requests, matches them against
    ///     registered endpoints via <see cref="IEndpointHostRouter"/>, and dispatches execution to
    ///     the matching endpoint's <see cref="ICompiledEndpointExecutor"/>. Requests that do not
    ///     match any active endpoint are forwarded to the next middleware in the pipeline.
    /// </summary>
    /// =================================================================================================
    public class EndpointHostBinderMiddleware
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the logger.
        /// </summary>
        /// =================================================================================================
        private readonly ILogger<EndpointHostBinderMiddleware> _logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the next.
        /// </summary>
        /// =================================================================================================
        private readonly RequestDelegate _next;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the endpoint host options.
        /// </summary>
        /// =================================================================================================
        private readonly EndpointHostOptions _options;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="EndpointHostBinderMiddleware" /> class.
        /// </summary>
        /// <param name="next">The next middleware delegate in the request pipeline.</param>
        /// <param name="logger">The logger used to record diagnostic and warning messages.</param>
        /// <param name="options">The endpoint host options controlling fall-through behaviour.</param>
        /// =================================================================================================
        public EndpointHostBinderMiddleware(
            RequestDelegate next, ILogger<EndpointHostBinderMiddleware> logger, EndpointHostOptions options)
        {
            _next = next;
            _logger = logger;
            _options = options ?? new EndpointHostOptions();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Processes the incoming HTTP request. If the router finds a matching active endpoint the
        ///     request is dispatched to its compiled executor; otherwise the request is forwarded to
        ///     the next middleware in the pipeline. Any unhandled exception is logged at the critical
        ///     level and re-thrown.
        /// </summary>
        /// <param name="context">The current HTTP context for the request being processed.</param>
        /// <param name="router">
        ///     The <see cref="IEndpointHostRouter"/> used to locate a matching endpoint.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> that completes when the request has been handled or forwarded.
        /// </returns>
        /// =================================================================================================
        public async Task InvokeAsync(HttpContext context, IEndpointHostRouter router)
        {
            context.ThrowIfArgNull(nameof(context));
            router.ThrowIfArgNull(nameof(router));

            try
            {
                var endpoint = router.Find(context);
                if (endpoint.IsNull())
                {
#if NET6_0_OR_GREATER
                    EndpointHostDiagnostics.RequestsNotMatched.Add(1);
#endif
                    await _next(context).ConfigureAwait(false);

                    return;
                }

                if (endpoint.Executor.IsNull())
                {
#if NET6_0_OR_GREATER
                    EndpointHostDiagnostics.RequestsNoExecutor.Add(1);
#endif
                    Log.NoExecutor(_logger, context.Request.Path, null);

                    if (_options.PassThroughOnNoExecutor)
                    {
                        await _next(context).ConfigureAwait(false);
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    }

                    return;
                }

#if NET6_0_OR_GREATER
                EndpointHostDiagnostics.RequestsMatched.Add(1);
#endif
                Log.ExecutingEndpoint(_logger, endpoint.Name, endpoint.Path, null);

                using (var activity = EndpointHostDiagnostics.ActivitySource.StartActivity(
                    endpoint.Name, ActivityKind.Internal))
                {
                    if (activity.IsNotNull())
                    {
                        activity!.SetTag("endpoint.name", endpoint.Name);
                        activity.SetTag("http.method", context.Request.Method);
                        activity.SetTag("http.route", endpoint.Path.Value);
                        activity.SetTag("endpoint.matched", true);
                    }

                    await endpoint.Executor.ExecuteAsync(context).ConfigureAwait(false);
                }

                return;
            }
            catch (Exception ex)
            {
                Log.UnhandledException(_logger, ex.Message, ex);

                throw;
            }
        }
    }
}
