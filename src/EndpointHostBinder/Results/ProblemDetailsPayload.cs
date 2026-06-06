// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:02
//  ***********************************************************************
//  <copyright file="ProblemDetailsPayload.cs" company="RzR SOFT & TECH">
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

namespace RzR.Infrastructure.EndpointHosting.Results
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Internal DTO that maps to the RFC 7807 problem-details object members.
    /// </summary>
    /// =================================================================================================
    internal sealed class ProblemDetailsPayload
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("title")] 
        public string Title { get; set; }

        [JsonPropertyName("status")] 
        public int Status { get; set; }

        [JsonPropertyName("detail")] 
        public string Detail { get; set; }

        [JsonPropertyName("instance")]
        public string Instance { get; set; }
    }
}