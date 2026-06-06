// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:05
//  ***********************************************************************
//  <copyright file="HttpContextRouteValuesExtensions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Http;
using RzR.Extensions.Domain.Validation;
using System.Collections.Generic;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Routing
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Extension methods for reading route parameter values that were captured by the
    ///     <see cref="RzR.Infrastructure.EndpointHosting.Host.EndpointHostRouter" /> when a templated
    ///     endpoint matched the current request.
    /// </summary>
    /// <remarks>
    ///     Values are stored in <see cref="HttpContext.Items" /> under the well-known key
    ///     <see cref="RouteValuesItemKey" />. For literal (non-templated) endpoints the key is
    ///     absent, and both extension methods return safe defaults (empty dictionary /
    ///     <see langword="null" />) so callers do not need to null-check before use.
    /// </remarks>
    /// =================================================================================================
    public static class HttpContextRouteValuesExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The well-known <see cref="HttpContext.Items" /> key under which the router stores
        ///     captured route parameter values after a templated endpoint match. Handlers and middleware may read values directly from
        ///     <see cref="HttpContext.Items" /> using this key if preferred.
        /// </summary>
        /// =================================================================================================
        public const string RouteValuesItemKey = "EndpointHostBinder.RouteValues";

        private static readonly IReadOnlyDictionary<string, string> _emptyValues
            = new Dictionary<string, string>();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns all route parameter values captured from the matched endpoint template, or an
        ///     empty dictionary when the matched endpoint was a literal (non-templated) path or no
        ///     endpoint matched at all.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>
        ///     An <see cref="IReadOnlyDictionary{TKey,TValue}" /> mapping parameter names to their
        ///     captured string values; never <see langword="null" />.
        /// </returns>
        /// =================================================================================================
        public static IReadOnlyDictionary<string, string> GetEndpointRouteValues(this HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            if (context.Items.TryGetValue(RouteValuesItemKey, out var raw)
                && raw is IReadOnlyDictionary<string, string> values)
                return values;

            return _emptyValues;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the captured value for a single route parameter, or <see langword="null" /> if
        ///     the parameter was not captured (literal endpoint, no match, or unknown name).
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="name">The route parameter name as declared in the template (e.g. <c>id</c>).</param>
        /// <returns>
        ///     The captured string value, or <see langword="null" /> if not found.
        /// </returns>
        /// =================================================================================================
        public static string GetEndpointRouteValue(this HttpContext context, string name)
        {
            context.ThrowIfArgNull(nameof(context));

            var values = context.GetEndpointRouteValues();

            return values.TryGetValue(name, out var value) ? value : null;
        }
    }
}