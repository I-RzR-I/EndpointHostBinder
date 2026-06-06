// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:55
//  ***********************************************************************
//  <copyright file="OpenApiInfo.cs" company="RzR SOFT & TECH">
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
    ///     Represents the <c>info</c> object in an OpenAPI document.
    /// </summary>
    /// =================================================================================================
    public sealed class OpenApiInfo
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the API title.
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the API version string.
        /// </summary>
        /// =================================================================================================
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}