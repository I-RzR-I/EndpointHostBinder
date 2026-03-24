// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2024-04-19 19:20
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 22:57
// ***********************************************************************
//  <copyright file="EndpointHostBinderMiddleware.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using DomainCommonExtensions.CommonExtensions;
using DomainCommonExtensions.Utilities.Ensure;
using EndpointHostBinder.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

#endregion

namespace EndpointHostBinder.Host
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
        ///     Initializes a new instance of the <see cref="EndpointHostBinderMiddleware" /> class.
        /// </summary>
        /// <param name="next">The next middleware delegate in the request pipeline.</param>
        /// <param name="logger">The logger used to record diagnostic and warning messages.</param>
        /// =================================================================================================
        public EndpointHostBinderMiddleware(RequestDelegate next, ILogger<EndpointHostBinderMiddleware> logger)
        {
            _next = next;
            _logger = logger;
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
                if (await router.ExistAsync(context).ConfigureAwait(false) == false)
                {
                    await _next(context).ConfigureAwait(false);

                    return;
                }

                var endpoint = router.Find(context);
                if (endpoint.IsNull() || endpoint.Executor.IsNull())
                {
                    _logger.LogWarning("Endpoint [{path}] has no compiled executor", context.Request.Path);
                    await _next(context).ConfigureAwait(false);

                    return;
                }

                _logger.LogDebug("Executing endpoint [{name}] for path [{path}]", endpoint.Name, endpoint.Path);
                await endpoint.Executor.ExecuteAsync(context).ConfigureAwait(false);
                
                return;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);

                throw;
            }
        }
    }
}