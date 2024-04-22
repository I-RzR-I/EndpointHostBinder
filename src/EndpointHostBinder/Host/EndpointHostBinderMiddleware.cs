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
using DomainCommonExtensions.DataTypeExtensions;
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
    ///     An endpoint host binder middleware.
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
        /// <param name="next">The next.</param>
        /// <param name="logger">The logger.</param>
        /// =================================================================================================
        public EndpointHostBinderMiddleware(RequestDelegate next, ILogger<EndpointHostBinderMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Executes the given operation on a different thread, and waits for the result.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="router">The router.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        public async Task Invoke(HttpContext context, IEndpointHostRouter router)
        {
            context.ThrowIfArgNull(nameof(context));
            router.ThrowIfArgNull(nameof(router));

            try
            {
                var endpoint = router.Find(context);
                if (endpoint.IsNotNull())
                {
                    _logger.LogInformation("Invoking endpoint: {endpointType} for {url}", endpoint.GetType().FullName, context.Request.Path.ToString());

                    var result = await endpoint.RequestProcessAsync(context);

                    if (result.IsNotNull())
                    {
                        _logger.LogTrace("Invoking result: {type}", result.GetType().FullName);
                        await result.ExecuteAsync(context);
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);

                throw;
            }

            await _next(context);
        }
    }
}