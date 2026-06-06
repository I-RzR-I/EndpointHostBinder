// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:55
//  ***********************************************************************
//  <copyright file="OpenApiDocument.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Collections.Generic;
using System.Text.Json.Serialization;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Models.OpenApi
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Represents the root OpenAPI 3.0.x document object.
    /// </summary>
    /// =================================================================================================
    public sealed class OpenApiDocument
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the OpenAPI specification version string (e.g. <c>3.0.1</c>).
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("openapi")]
        public string Openapi { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the document metadata (title and version).
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("info")]
        public OpenApiInfo Info { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the path items, keyed by path string (e.g. <c>/api/resource</c>).
        ///     Each value maps lowercase HTTP method verbs to their operation objects.
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("paths")]
        public Dictionary<string, Dictionary<string, OpenApiOperation>> Paths { get; set; }
    }
}