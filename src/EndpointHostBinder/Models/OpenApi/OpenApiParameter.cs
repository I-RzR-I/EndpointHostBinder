// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:55
//  ***********************************************************************
//  <copyright file="OpenApiParameter.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Text.Json.Serialization;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Models.OpenApi
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Represents a single parameter (path, query, header, or cookie) in an OpenAPI operation.
    /// </summary>
    /// =================================================================================================
    public sealed class OpenApiParameter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the parameter name as it appears in the path template (without braces).
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the location of the parameter: <c>path</c>, <c>query</c>,
        ///     <c>header</c>, or <c>cookie</c>.
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("in")]
        public string In { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets a value indicating whether the parameter is mandatory.
        ///     Path parameters are always required per the OpenAPI specification.
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("required")]
        public bool Required { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the JSON Schema that describes the parameter value type.
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("schema")]
        public OpenApiSchema Schema { get; set; }
    }
}