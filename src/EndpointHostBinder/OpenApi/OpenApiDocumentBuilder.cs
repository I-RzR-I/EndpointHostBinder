// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:55
//  ***********************************************************************
//  <copyright file="OpenApiDocumentBuilder.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using RzR.Extensions.Domain.Collections;
using RzR.Extensions.Domain.Primitives;
using RzR.Extensions.Domain.Text;
using RzR.Extensions.Domain.Validation;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Models.OpenApi;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#endregion

namespace RzR.Infrastructure.EndpointHosting.OpenApi
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Builds a minimal but valid OpenAPI 3.0.x document from the endpoints registered with an
    ///     <see cref="IEndpointHostRouter" />. The document is self-contained and produced with
    ///     System.Text.Json — no Swashbuckle or other third-party dependency is required.
    /// </summary>
    /// =================================================================================================
    public sealed class OpenApiDocumentBuilder
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The default document title used when none is supplied.
        /// </summary>
        /// =================================================================================================
        public const string DefaultTitle = "EndpointHostBinder API";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The default document version used when none is supplied.
        /// </summary>
        /// =================================================================================================
        public const string DefaultVersion = "1.0.0";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
            WriteIndented = false
        };

        private static readonly IReadOnlyList<string> _noParams = new List<string>(0);

        private readonly IEndpointHostRouter _router;
        private readonly string _title;
        private readonly string _version;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="OpenApiDocumentBuilder" /> class with
        ///     default document title and version.
        /// </summary>
        /// <param name="router">
        ///     The router whose registered endpoints will be reflected into the OpenAPI document.
        /// </param>
        /// =================================================================================================
        public OpenApiDocumentBuilder(IEndpointHostRouter router)
            : this(router, DefaultTitle, DefaultVersion)
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="OpenApiDocumentBuilder" /> class with a
        ///     configurable document title and version.
        /// </summary>
        /// <param name="router">
        ///     The router whose registered endpoints will be reflected into the OpenAPI document.
        /// </param>
        /// <param name="title">
        ///     The API title placed in the document's <c>info.title</c> field.
        /// </param>
        /// <param name="version">
        ///     The API version placed in the document's <c>info.version</c> field.
        /// </param>
        /// =================================================================================================
        public OpenApiDocumentBuilder(IEndpointHostRouter router, string title, string version)
        {
            router.ThrowIfArgNull(nameof(router));

            _router = router;
            _title = string.IsNullOrEmpty(title) ? DefaultTitle : title;
            _version = string.IsNullOrEmpty(version) ? DefaultVersion : version;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds the OpenAPI 3.0.1 document as a strongly-typed object graph.
        /// </summary>
        /// <remarks>
        ///     By default inactive endpoints (where <see cref="Models.Endpoint.IsActive" /> is
        ///     <see langword="false" />) are excluded so the document reflects the set of endpoints
        ///     that are actually routable. Pass <paramref name="includeInactive" /> as
        ///     <see langword="true" /> to include all registered endpoints regardless of their active
        ///     state, which is useful for documentation or administrative tooling.
        /// </remarks>
        /// <param name="includeInactive">
        ///     When <see langword="true" />, inactive endpoints are included in the document.
        ///     Defaults to <see langword="false" /> (inactive endpoints are excluded).
        /// </param>
        /// <returns>
        ///     An <see cref="OpenApiDocument" /> that represents the matching registered endpoints.
        /// </returns>
        /// =================================================================================================
        public OpenApiDocument Build(bool includeInactive = false)
        {
            var endpoints = _router.GetEndpoints();
            var paths = new Dictionary<string, Dictionary<string, OpenApiOperation>>();

            foreach (var endpoint in endpoints)
            {
                if (includeInactive.IsFalse() && endpoint.IsActive.IsFalse()) continue;

                var path = endpoint.Path.Value ?? "/";

                if (!paths.TryGetValue(path, out var pathItem))
                {
                    pathItem = new Dictionary<string, OpenApiOperation>();
                    paths[path] = pathItem;
                }

                var pathParams = ExtractPathParameters(path);

                if (endpoint.AllowedMethods.IsNull())
                {
                    // No method constraint — emit a single GET operation.
                    if (!pathItem.ContainsKey("get"))
                        pathItem["get"] = BuildOperation(endpoint.Name, pathParams);
                }
                else
                {
                    foreach (var method in endpoint.AllowedMethods)
                    {
                        var verb = method.Method.ToLowerInvariant();
                        if (!pathItem.ContainsKey(verb))
                            pathItem[verb] = BuildOperation(endpoint.Name, pathParams);
                    }
                }
            }

            return new OpenApiDocument
            {
                Openapi = "3.0.1", 
                Info = new OpenApiInfo
                {
                    Title = _title, 
                    Version = _version
                }, 
                Paths = paths
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds the OpenAPI 3.0.1 document and serializes it to a JSON string using
        ///     System.Text.Json.
        /// </summary>
        /// <param name="includeInactive">
        ///     When <see langword="true" />, inactive endpoints are included in the document.
        ///     Defaults to <see langword="false" /> (inactive endpoints are excluded).
        ///     Forwarded directly to <see cref="Build(bool)" />.
        /// </param>
        /// <returns>A JSON string representing the complete OpenAPI document.</returns>
        /// =================================================================================================
        public string BuildJson(bool includeInactive = false)
            => JsonSerializer.Serialize(Build(includeInactive), SerializerOptions);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds a minimal <see cref="OpenApiOperation" /> for a single endpoint, containing
        ///     an <c>operationId</c>, an optional <c>parameters</c> array for path parameters, and
        ///     a generic 200 response.
        /// </summary>
        /// <param name="operationId">
        ///     The value to place in the operation's <c>operationId</c> field, derived from the
        ///     endpoint name.
        /// </param>
        /// <param name="pathParameters">
        ///     The names of path parameters extracted from the endpoint path template (e.g.
        ///     <c>id</c> for <c>/users/{id}</c>).  Pass <see langword="null" /> or an empty list
        ///     for literal endpoints — no <c>parameters</c> array will be emitted.
        /// </param>
        /// <returns>A new <see cref="OpenApiOperation" /> instance.</returns>
        /// =================================================================================================
        private static OpenApiOperation BuildOperation(string operationId, IReadOnlyList<string> pathParameters = null)
        {
            List<OpenApiParameter> parameters = null;

            if (pathParameters.IsNotNullOrEmptyEnumerable())
            {
                parameters = new List<OpenApiParameter>(pathParameters!.Count);
                foreach (var name in pathParameters)
                {
                    parameters.Add(new OpenApiParameter
                    {
                        Name = name,
                        In = "path",
                        Required = true, 
                        Schema = new OpenApiSchema
                        {
                            Type = "string"
                        }
                    });
                }
            }

            return new OpenApiOperation
            {
                OperationId = operationId, 
                Parameters = parameters, 
                Responses = new Dictionary<string, OpenApiResponse>
                {
                    ["200"] = new() { Description = "Success" }
                }
            };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Extracts the ordered parameter names from an OpenAPI / route template path string.
        ///     Returns an empty list for literal paths that contain no <c>{name}</c> segments.
        /// </summary>
        /// <param name="path">
        ///     The endpoint path value (e.g. <c>/users/{id}</c> or <c>/orders/{orderId}/items/{itemId}</c>).
        /// </param>
        /// <returns>
        ///     An ordered list of parameter name strings; empty when the path has no parameters.
        /// </returns>
        /// =================================================================================================
        private static IReadOnlyList<string> ExtractPathParameters(string path)
        {
            if (path.IsMissing() || path.IndexOf('{') < 0)
                return _noParams;

            var names = new List<string>();
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var seg in segments)
            {
                if (seg.Length >= 2 && seg[0] == '{' && seg[seg.Length - 1] == '}')
                    names.Add(seg.Substring(1, seg.Length - 2));
            }

            return names;
        }
    }
}