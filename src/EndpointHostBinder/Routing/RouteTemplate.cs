// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 21:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:05
//  ***********************************************************************
//  <copyright file="RouteTemplate.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using RzR.Extensions.Domain.Text;
using RzR.Infrastructure.EndpointHosting.Models;
using System;
using System.Collections.Generic;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Routing
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Represents a precompiled route template for a single-segment-parameter path such as
    ///     <c>/users/{id}</c> or <c>/orders/{orderId}/items/{itemId}</c>. Literal segments are
    ///     compared using the <see cref="StringComparison" /> derived from
    ///     <see cref="Configuration.EndpointHostOptions.PathComparison" />. Parameter segments
    ///     (surrounded by <c>{</c> and <c>}</c>) capture the corresponding request path segment
    ///     into a name-to-value dictionary.
    /// </summary>
    /// <remarks>
    ///     Catch-all parameters (<c>{*rest}</c>) are <b>not</b> supported. If a template segment
    ///     begins with <c>*</c> after the opening brace (e.g. <c>{*rest}</c>) the asterisk is
    ///     treated as part of the parameter name — it will never match a multi-segment path.
    ///     Specificity is measured by <see cref="LiteralCount" />: a template with more literal
    ///     segments is preferred over one with fewer when multiple templates match the same
    ///     request path. This keeps precedence deterministic and simple.
    /// </remarks>
    /// =================================================================================================
    internal sealed class RouteTemplate
    {
        private readonly string[] _literals;
        private readonly string[] _paramNames;
        private readonly bool[] _isCapture;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new <see cref="RouteTemplate" /> by parsing the path stored on
        ///     <paramref name="endpoint" /> into segment descriptors that can be evaluated at request
        ///     time without further string allocations beyond the captured values.
        /// </summary>
        /// <param name="endpoint">
        ///     The registered endpoint whose <see cref="Models.Endpoint.Path" /> contains the template.
        /// </param>
        /// =================================================================================================
        internal RouteTemplate(Endpoint endpoint)
        {
            Endpoint = endpoint;

            var rawPath = endpoint.Path.Value ?? string.Empty;

            // Split into segments, discarding leading slash (PathString always starts with /).
            var rawSegments = rawPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var segmentCount = rawSegments.Length;

            _literals = new string[segmentCount];
            _paramNames = new string[segmentCount];
            _isCapture = new bool[segmentCount];

            var literalCount = 0;
            var paramList = new List<string>();

            for (var i = 0; i < segmentCount; i++)
            {
                var seg = rawSegments[i];
                if (seg.Length >= 2 && seg[0] == '{' && seg[seg.Length - 1] == '}')
                {
                    // Capture segment — name is everything between the braces.
                    var name = seg.Substring(1, seg.Length - 2);
                    if (name.IsMissing())
                        throw new ArgumentException(
                            $"Route template '{rawPath}' contains an empty parameter name in segment '{seg}'. Parameter names must be non-empty.",
                            nameof(endpoint));
                    if (paramList.Contains(name))
                        throw new ArgumentException(
                            $"Route template '{rawPath}' declares a duplicate parameter name '{name}'. Parameter names must be unique within a template.",
                            nameof(endpoint));
                    _isCapture[i] = true;
                    _paramNames[i] = name;
                    paramList.Add(name);
                }
                else
                {
                    _isCapture[i] = false;
                    _literals[i] = seg;
                    literalCount++;
                }
            }

            LiteralCount = literalCount;
            ParameterNames = paramList.AsReadOnly();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the <see cref="Endpoint" /> associated with this template.
        /// </summary>
        /// =================================================================================================
        internal Endpoint Endpoint { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the number of literal segments in this template.  Used as a specificity score:
        ///     higher is more specific (preferred over templates with more capture segments).
        /// </summary>
        /// =================================================================================================
        internal int LiteralCount { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the ordered names of the path parameters declared in the template.  The order
        ///     matches the segment positions in the template path, skipping literal segments.
        /// </summary>
        /// =================================================================================================
        internal IReadOnlyList<string> ParameterNames { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Determines whether <paramref name="requestSegments" /> match this template and, when
        ///     they do, populates <paramref name="values" /> with the captured parameter values.
        /// </summary>
        /// <param name="requestSegments">
        ///     The non-empty path segments split from the incoming request path. The segment count
        ///     must equal the template's segment count for a match.
        /// </param>
        /// <param name="comparison">
        ///     The <see cref="StringComparison" /> used for literal segment matching, derived from
        ///     <see cref="Configuration.EndpointHostOptions.PathComparison" />.
        /// </param>
        /// <param name="values">
        ///     When the method returns <see langword="true" />, contains the captured
        ///     parameter values keyed by parameter name; otherwise the dictionary is empty.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if every segment matches; otherwise <see langword="false" />.
        /// </returns>
        /// =================================================================================================
        internal bool TryMatch(
            string[] requestSegments, StringComparison comparison,
            out Dictionary<string, string> values)
        {
            values = new Dictionary<string, string>(StringComparer.Ordinal);

            if (requestSegments.Length != _literals.Length)
                return false;

            for (var i = 0; i < _literals.Length; i++)
            {
                if (_isCapture[i])
                {
                    // Capture segment — record the value.
                    values[_paramNames[i]] = requestSegments[i];
                }
                else
                {
                    // Literal segment — must match with configured comparison.
                    if (!string.Equals(_literals[i], requestSegments[i], comparison))
                        return false;
                }
            }

            return true;
        }
    }
}