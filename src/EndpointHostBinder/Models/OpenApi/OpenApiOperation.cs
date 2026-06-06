// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:55
//  ***********************************************************************
//  <copyright file="OpenApiOperation.cs" company="RzR SOFT & TECH">
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
    ///     Represents a single operation (method + path combination) in an OpenAPI path item.
    /// </summary>
    /// =================================================================================================
    public sealed class OpenApiOperation
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the unique string used to identify the operation.
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("operationId")]
        public string OperationId { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the path (and other) parameters for this operation.
        ///     <see langword="null" /> when the endpoint has no path parameters; the JSON serializer
        ///     omits the field entirely in that case due to
        ///     <see cref="JsonIgnoreCondition.WhenWritingNull" />.
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("parameters")]
        public List<OpenApiParameter> Parameters { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the response objects keyed by HTTP status code string (e.g. <c>"200"</c>).
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("responses")]
        public Dictionary<string, OpenApiResponse> Responses { get; set; }
    }
}