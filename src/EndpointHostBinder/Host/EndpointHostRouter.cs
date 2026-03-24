// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2024-04-19 18:52
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 22:57
// ***********************************************************************
//  <copyright file="EndpointHostRouter.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using DomainCommonExtensions.ArraysExtensions;
using DomainCommonExtensions.CommonExtensions;
using DomainCommonExtensions.DataTypeExtensions;
using DomainCommonExtensions.Utilities.Ensure;
using EndpointHostBinder.Abstractions;
using EndpointHostBinder.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

#endregion

namespace EndpointHostBinder.Host
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Routes incoming HTTP requests to the matching registered <see cref="Endpoint"/> by
    ///     comparing the request path and HTTP method against the collection of endpoints provided
    ///     at construction time.
    /// </summary>
    /// <seealso cref="T:EndpointHostBinder.Abstractions.IEndpointHostRouter" />
    /// =================================================================================================
    public sealed class EndpointHostRouter : IEndpointHostRouter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the endpoints.
        /// </summary>
        /// =================================================================================================
        private readonly IEnumerable<Endpoint> _endpoints;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the logger.
        /// </summary>
        /// =================================================================================================
        private readonly ILogger<EndpointHostRouter> _logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="EndpointHostRouter" /> class.
        /// </summary>
        /// <param name="endpoints">
        ///     The collection of registered <see cref="Endpoint"/> instances to route against.
        /// </param>
        /// <param name="logger">The logger used to record diagnostic and warning messages.</param>
        /// =================================================================================================
        public EndpointHostRouter(IEnumerable<Endpoint> endpoints, ILogger<EndpointHostRouter> logger)
        {
            _endpoints = endpoints;
            _logger = logger;
        }

        /// <inheritdoc />
        public Endpoint Find(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            var endpoint = _endpoints.FirstOrDefault(x =>
                x.Path.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase) &&
                (x.AllowedMethods.IsNullOrEmptyEnumerable() ||
                 x.AllowedMethods.Any(m => m == new HttpMethod(context.Request.Method))));

            if (endpoint.IsNull())
            {
                _logger.LogDebug("Request path [{path}] no match any endpoints", context.Request.Path);

                return null;
            }

            _logger.LogDebug("Request path [{path}] matched endpoint [{endpoint}]", context.Request.Path, endpoint!.Name);

            if (endpoint.IsActive.IsFalse())
            {
                _logger.LogWarning("Endpoint disabled: [{endpoint}]", endpoint.Name);

                return null;
            }

            _logger.LogDebug("Endpoint enabled: [{endpoint}], successfully find handler: [{endpointHandler}]", endpoint.Name, endpoint.EndpointType.FullName);

            return endpoint;
        }

        /// <inheritdoc />
        public bool Exist(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));
            return _endpoints.Any(x =>
                x.IsActive &&
                x.Path.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase) &&
                (x.AllowedMethods.IsNullOrEmptyEnumerable() ||
                 x.AllowedMethods.Any(m => m == new HttpMethod(context.Request.Method))));
        }

        /// <inheritdoc />
        public Task<bool> ExistAsync(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            return Task.FromResult(Exist(context));
        }
    }
}