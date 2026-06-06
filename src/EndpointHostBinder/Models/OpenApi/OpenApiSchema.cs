// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:55
//  ***********************************************************************
//  <copyright file="OpenApiSchema.cs" company="RzR SOFT & TECH">
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
    ///     Represents a minimal JSON Schema object used to describe a parameter's value type.
    /// </summary>
    /// =================================================================================================
    public sealed class OpenApiSchema
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the JSON Schema primitive type (e.g. <c>string</c>, <c>integer</c>).
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}