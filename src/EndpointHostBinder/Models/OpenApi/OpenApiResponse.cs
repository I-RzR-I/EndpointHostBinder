// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:55
//  ***********************************************************************
//  <copyright file="OpenApiResponse.cs" company="RzR SOFT & TECH">
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
    ///     Represents a single response entry in an OpenAPI operation's <c>responses</c> map.
    /// </summary>
    /// =================================================================================================
    public sealed class OpenApiResponse
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the human-readable description of this response.
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}