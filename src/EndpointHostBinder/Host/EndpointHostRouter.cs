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

using DomainCommonExtensions.DataTypeExtensions;
using EndpointHostBinder.Abstractions;
using EndpointHostBinder.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace EndpointHostBinder.Host
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An endpoint host router.
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
        /// <param name="endpoints">The endpoints.</param>
        /// <param name="logger">The logger.</param>
        /// =================================================================================================
        public EndpointHostRouter(IEnumerable<Endpoint> endpoints, ILogger<EndpointHostRouter> logger)
        {
            _endpoints = endpoints;
            _logger = logger;
        }

        /// <inheritdoc />
        public IEndpointHostRequestHandler Find(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            if (Exist(context).IsFalse())
            {
                _logger.LogDebug("Request path [{path}] no match any endpoints", context.Request.Path);

                return null;
            }

            var endpoint = _endpoints.FirstOrDefault(x => x.Path.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase));
            _logger.LogDebug("Request path [{path}] matched to endpoint type [{endpoint}]", context.Request.Path, endpoint!.Name);

            if (endpoint.IsActive.IsTrue())
            {
                var handler = context.RequestServices.GetService(endpoint.EndpointType);
                if (handler is IEndpointHostRequestHandler hostHandler)
                {
                    _logger.LogDebug("Endpoint enabled: [{endpoint}], successfully created handler: [{endpointHandler}]", endpoint.Name, endpoint.EndpointType.FullName);

                    return hostHandler;
                }

                _logger.LogDebug("Endpoint enabled: [{endpoint}], failed to create handler: [{endpointHandler}]", endpoint.Name, endpoint.EndpointType.FullName);
            }

            _logger.LogWarning("Endpoint disabled: [{endpoint}]", endpoint.Name);

            return null;
        }

        /// <inheritdoc />
        public bool Exist(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            return _endpoints.Any(x => x.Path.Equals(context.Request.Path, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public async Task<bool> ExistAsync(HttpContext context)
            => await Task.Run(() => Exist(context));
    }
}