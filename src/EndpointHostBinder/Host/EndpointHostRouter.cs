// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2024-04-19 18:52
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-04 23:44
// ***********************************************************************
//  <copyright file="EndpointHostRouter.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RzR.Extensions.Domain.Collections;
using RzR.Extensions.Domain.Primitives;
using RzR.Extensions.Domain.Validation;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Configuration;
using RzR.Infrastructure.EndpointHosting.Diagnostics;
using RzR.Infrastructure.EndpointHosting.Models;
using RzR.Infrastructure.EndpointHosting.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Host
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Routes incoming HTTP requests to the matching registered <see cref="Endpoint"/> by
    ///     comparing the request path and HTTP method against the collection of endpoints provided
    ///     at construction time.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <b>Literal (exact-match) endpoints</b> are indexed into a path-keyed dictionary at
    ///         construction time, giving O(1) lookup per request.  Each bucket contains all
    ///         endpoints registered under the same normalised path so that method-specific routing
    ///         on a shared path works correctly.
    ///     </para>
    ///     <para>
    ///         <b>Templated endpoints</b> — those whose path contains at least one <c>{name}</c>
    ///         segment — are precompiled into <see cref="RouteTemplate"/> instances once at
    ///         construction time.  On a request that misses the literal dictionary the router
    ///         evaluates each template by splitting the request path into segments and matching
    ///         segment-by-segment (segment count must match; literal segments are compared with the
    ///         configured <see cref="StringComparison"/>; <c>{name}</c> captures the value).
    ///     </para>
    ///     <para>
    ///         <b>Precedence rules (deterministic):</b>
    ///         <list type="number">
    ///             <item>Literal/exact matches always win over any templated match.</item>
    ///             <item>
    ///                 Among multiple matching templates the most specific one wins: the template
    ///                 with the most literal segments (fewest capture segments) is preferred.
    ///             </item>
    ///             <item>
    ///                 Within the winning specificity bucket, the first registered active endpoint
    ///                 whose HTTP method constraint matches wins (consistent with the literal path
    ///                 behaviour).
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Captured route values are written to <see cref="HttpContext.Items"/> under
    ///         <see cref="HttpContextRouteValuesExtensions.RouteValuesItemKey"/> as an
    ///         <see cref="IReadOnlyDictionary{TKey,TValue}"/> of <c>string -> string</c>.  For
    ///         literal matches no entry is written.  Callers should use
    ///         <see cref="HttpContextRouteValuesExtensions.GetEndpointRouteValues"/> or
    ///         <see cref="HttpContextRouteValuesExtensions.GetEndpointRouteValue"/> to read values.
    ///     </para>
    ///     <para>
    ///         HTTP method names are compared with
    ///         <see cref="StringComparison.OrdinalIgnoreCase"/> — no <c>HttpMethod</c> allocation
    ///         occurs per request.
    ///     </para>
    /// </remarks>
    /// <seealso cref="T:RzR.Infrastructure.EndpointHosting.Abstractions.IEndpointHostRouter" />
    /// =================================================================================================
    public sealed class EndpointHostRouter : IEndpointHostRouter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) path-to-endpoint-bucket lookup built once at construction time.
        ///     Contains only literal (non-templated) endpoints.
        ///     Each bucket holds all endpoints registered under a given normalised path.
        /// </summary>
        /// =================================================================================================
        private readonly Dictionary<string, List<Endpoint>> _lookup;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) precompiled route templates for all endpoints whose path contains a
        ///     <c>{name}</c> segment, sorted descending by <see cref="RouteTemplate.LiteralCount"/>
        ///     so the most specific template is evaluated first.
        /// </summary>
        /// =================================================================================================
        private readonly List<RouteTemplate> _templates;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the <see cref="StringComparison"/> configured via
        ///     <see cref="EndpointHostOptions.PathComparison"/> — used for literal-segment matching
        ///     inside template evaluation.
        /// </summary>
        /// =================================================================================================
        private readonly StringComparison _pathComparison;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) flat snapshot of all registered endpoints, in registration order.
        ///     Exposed by <see cref="GetEndpoints"/> for metadata consumers such as the OpenAPI
        ///     document builder.
        /// </summary>
        /// =================================================================================================
        private readonly IReadOnlyCollection<Endpoint> _allEndpoints;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the logger.
        /// </summary>
        /// =================================================================================================
        private readonly ILogger<EndpointHostRouter> _logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="EndpointHostRouter" /> class using
        ///     default <see cref="EndpointHostOptions"/>.
        /// </summary>
        /// <param name="endpoints">
        ///     The collection of registered <see cref="Endpoint"/> instances to route against.
        ///     The collection is snapshotted into a lookup dictionary at construction time.
        /// </param>
        /// <param name="logger">The logger used to record diagnostic and warning messages.</param>
        /// =================================================================================================
        public EndpointHostRouter(IEnumerable<Endpoint> endpoints, ILogger<EndpointHostRouter> logger)
            : this(endpoints, logger, new EndpointHostOptions()) { }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="EndpointHostRouter" /> class with
        ///     explicit <see cref="EndpointHostOptions"/>.
        /// </summary>
        /// <param name="endpoints">
        ///     The collection of registered <see cref="Endpoint"/> instances to route against.
        ///     The collection is snapshotted into a lookup dictionary at construction time.
        /// </param>
        /// <param name="logger">The logger used to record diagnostic and warning messages.</param>
        /// <param name="options">
        ///     The <see cref="EndpointHostOptions"/> that control path-matching case-sensitivity
        ///     and fall-through behaviour.
        /// </param>
        /// =================================================================================================
        public EndpointHostRouter(IEnumerable<Endpoint> endpoints, ILogger<EndpointHostRouter> logger,
            EndpointHostOptions options)
        {
            endpoints.ThrowIfArgNull(nameof(endpoints));
            logger.ThrowIfArgNull(nameof(logger));
            options.ThrowIfArgNull(nameof(options));

            _logger = logger;
            _pathComparison = options.PathComparison;

            // Materialise once so both the flat snapshot and the lookup/template structures share
            // the same enumeration pass and the IEnumerable is not read twice.
            var snapshot = new List<Endpoint>(endpoints);
            _allEndpoints = snapshot.AsReadOnly();

            BuildStructures(snapshot, options.PathComparison, out _lookup, out _templates);
        }

        /// <inheritdoc />
        public Endpoint Find(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            return Find(context, true);
        }

        /// <inheritdoc />
        public bool Exist(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            return Find(context, false).IsNotNull();
        }

        /// <inheritdoc />
        public Task<bool> ExistAsync(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            return Task.FromResult(Find(context, false).IsNotNull());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Core match implementation shared by <see cref="Find(HttpContext)" /> and
        ///     <see cref="Exist" />/<see cref="ExistAsync" />. When <paramref name="writeRouteValues" />
        ///     is <see langword="true" /> the captured route-value dictionary is written to
        ///     <see cref="HttpContext.Items" />; when <see langword="false" /> the match is performed
        ///     without any side-effects on the context.
        /// </summary>
        /// <param name="context">The HTTP context for the current request. Must be non-null.</param>
        /// <param name="writeRouteValues">
        ///     <see langword="true" /> to write captured route values into
        ///     <see cref="HttpContext.Items" /> (used by <see cref="Find(HttpContext)" />);
        ///     <see langword="false" /> for a side-effect-free existence check.
        /// </param>
        /// <returns>The matched <see cref="Endpoint"/>, or <see langword="null"/> if none matched.</returns>
        /// =================================================================================================
        private Endpoint Find(HttpContext context, bool writeRouteValues)
        {
            var requestPath = context.Request.Path.Value;
            var requestMethod = context.Request.Method;

            if (_lookup.TryGetValue(requestPath, out var bucket))
            {
                var literalMatch = FindInBucket(bucket, requestMethod);
                if (literalMatch.IsNotNull())
                {
                    Log.EndpointFoundLiteral(_logger, literalMatch.Name, literalMatch.EndpointType.FullName, null);

                    return literalMatch;
                }
            }

            if (_templates.Count > 0)
            {
                var requestSegments = SplitPath(requestPath);

                foreach (var template in _templates)
                {
                    if (!template.TryMatch(requestSegments, _pathComparison, out var captured))
                        continue;

                    var endpoint = template.Endpoint;
                    if (!endpoint.IsActive)
                        continue;

                    if (!MethodMatches(endpoint, requestMethod))
                        continue;

                    if (writeRouteValues)
                    {
                        // Write captured values into HttpContext.Items before returning.
                        context.Items[HttpContextRouteValuesExtensions.RouteValuesItemKey] =
                            (IReadOnlyDictionary<string, string>)captured;
                    }

                    Log.EndpointFoundTemplate(_logger, endpoint.Name, endpoint.EndpointType.FullName, null);

                    return endpoint;
                }
            }

            Log.NoMatchFound(_logger, context.Request.Path, null);

            return null;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Endpoint> GetEndpoints() => _allEndpoints;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds the literal lookup dictionary and the sorted template list from
        ///     <paramref name="endpoints"/>, separating each endpoint into the appropriate
        ///     structure based on whether its path contains a <c>{name}</c> segment.
        /// </summary>
        /// <param name="endpoints">All registered endpoints.</param>
        /// <param name="pathComparison">
        ///     The configured path comparison, used both for the dictionary key comparer and for
        ///     the initial sort of templates (templates themselves use it at match time).
        /// </param>
        /// <param name="lookup">Receives the literal-path dictionary.</param>
        /// <param name="templates">
        ///     Receives the template list, sorted descending by
        ///     <see cref="RouteTemplate.LiteralCount"/> (most specific first).
        /// </param>
        /// =================================================================================================
        private static void BuildStructures(IEnumerable<Endpoint> endpoints, StringComparison pathComparison,
            out Dictionary<string, List<Endpoint>> lookup, out List<RouteTemplate> templates)
        {
            var comparer = GetComparerFor(pathComparison);
            lookup = new Dictionary<string, List<Endpoint>>(comparer);
            templates = new List<RouteTemplate>();

            foreach (var ep in endpoints)
            {
                var rawPath = ep.Path.Value;

                if (IsTemplated(rawPath))
                {
                    templates.Add(new RouteTemplate(ep));
                }
                else
                {
                    if (!lookup.TryGetValue(rawPath, out var bucket))
                    {
                        bucket = new List<Endpoint>();
                        lookup[rawPath] = bucket;
                    }

                    bucket.Add(ep);
                }
            }

            // Sort most-specific (most literal segments) first so the first hit is the winner.
            templates.Sort((a, b) => b.LiteralCount.CompareTo(a.LiteralCount));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the first active endpoint in <paramref name="bucket"/> whose
        ///     <c>AllowedMethods</c> constraint matches <paramref name="requestMethod"/>, or
        ///     <see langword="null"/> when no such endpoint exists.
        /// </summary>
        /// <remarks>
        ///     <c>IsActive</c> is evaluated at lookup time (not pre-filtered during construction) so
        ///     that the active-wins shadowing semantics are preserved for endpoints registered under
        ///     the same path.  HTTP method names are compared with
        ///     <see cref="StringComparison.OrdinalIgnoreCase"/> — the HTTP specification treats
        ///     method tokens as case-insensitive.
        /// </remarks>
        /// <param name="bucket">The candidate endpoints registered under a single path.</param>
        /// <param name="requestMethod">The HTTP method string from the incoming request.</param>
        /// <returns>The first matching active <see cref="Endpoint"/>, or <see langword="null"/>.</returns>
        /// =================================================================================================
        private static Endpoint FindInBucket(List<Endpoint> bucket, string requestMethod)
            => bucket.FirstOrDefault(x => x.IsActive && MethodMatches(x, requestMethod));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns <see langword="true"/> if <paramref name="endpoint"/> accepts
        ///     <paramref name="requestMethod"/> — either because no method constraint is declared
        ///     or because the constraint list contains a matching token.
        /// </summary>
        /// <param name="endpoint">The endpoint to test.</param>
        /// <param name="requestMethod">The HTTP method from the incoming request.</param>
        /// <returns><see langword="true"/> if the method is accepted; otherwise <see langword="false"/>.</returns>
        /// =================================================================================================
        private static bool MethodMatches(Endpoint endpoint, string requestMethod)
            => endpoint.AllowedMethods.IsNullOrEmptyEnumerable() ||
               endpoint.AllowedMethods.Any(m =>
                   string.Equals(m.Method, requestMethod, StringComparison.OrdinalIgnoreCase));

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns <see langword="true"/> if <paramref name="path"/> contains at least one
        ///     <c>{name}</c> segment and therefore should be treated as a route template.
        /// </summary>
        /// <param name="path">The raw path value from <see cref="Endpoint.Path"/>.</param>
        /// <returns><see langword="true"/> when the path is templated; <see langword="false"/> otherwise.</returns>
        /// =================================================================================================
        private static bool IsTemplated(string path)
            => path.IndexOf('{') >= 0;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Splits <paramref name="requestPath"/> on <c>/</c>, discarding empty entries produced
        ///     by the leading slash that ASP.NET Core always prepends to
        ///     <see cref="HttpRequest.Path"/>.
        /// </summary>
        /// <param name="requestPath">The raw path string from the request.</param>
        /// <returns>A non-null array of non-empty segment strings.</returns>
        /// =================================================================================================
        private static string[] SplitPath(string requestPath)
            => (requestPath ?? string.Empty).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the <see cref="StringComparer"/> that corresponds to the given
        ///     <see cref="StringComparison"/> value so that the dictionary key-comparer and the
        ///     individual string comparisons use identical semantics.
        /// </summary>
        /// <param name="comparison">The <see cref="StringComparison"/> to map.</param>
        /// <returns>A <see cref="StringComparer"/> matching <paramref name="comparison"/>.</returns>
        /// =================================================================================================
        private static StringComparer GetComparerFor(StringComparison comparison)
        {
            switch (comparison)
            {
                case StringComparison.Ordinal:
                    return StringComparer.Ordinal;
                case StringComparison.OrdinalIgnoreCase:
                    return StringComparer.OrdinalIgnoreCase;
                case StringComparison.InvariantCulture:
                    return StringComparer.InvariantCulture;
                case StringComparison.InvariantCultureIgnoreCase:
                    return StringComparer.InvariantCultureIgnoreCase;
                case StringComparison.CurrentCulture:
                    return StringComparer.CurrentCulture;
                case StringComparison.CurrentCultureIgnoreCase:
                    return StringComparer.CurrentCultureIgnoreCase;
                default:
                    return StringComparer.OrdinalIgnoreCase;
            }
        }
    }
}
