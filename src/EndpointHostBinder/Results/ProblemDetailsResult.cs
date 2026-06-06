// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:01
//  ***********************************************************************
//  <copyright file="ProblemDetailsResult.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Http;
using RzR.Extensions.Domain.Validation;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Results
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An <see cref="IEndpointHostResult" /> that writes an RFC 7807 Problem Details response
    ///     (<c>application/problem+json</c>) with configurable <c>type</c>, <c>title</c>,
    ///     <c>status</c>, <c>detail</c>, and <c>instance</c> fields.
    /// </summary>
    /// <seealso cref="T:RzR.Infrastructure.EndpointHosting.Abstractions.IEndpointHostResult" />
    /// =================================================================================================
    public sealed class ProblemDetailsResult : IEndpointHostResult
    {
        private const string ProblemJsonContentType = "application/problem+json; charset=utf-8";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProblemDetailsResult" /> class.
        /// </summary>
        /// <param name="title">A short human-readable summary of the problem type.</param>
        /// <param name="detail">
        ///     (Optional) A human-readable explanation of this specific occurrence of the problem.
        /// </param>
        /// <param name="statusCode">
        ///     (Optional) The HTTP status code. Defaults to 500.
        /// </param>
        /// <param name="type">
        ///     (Optional) A URI reference that identifies the problem type.
        /// </param>
        /// <param name="instance">
        ///     (Optional) A URI reference that identifies the specific occurrence of the problem.
        /// </param>
        /// =================================================================================================
        public ProblemDetailsResult(string title, string detail = null, int statusCode = 500,
            string type = null, string instance = null)
        {
            Title = title ?? string.Empty;
            Detail = detail;
            Status = statusCode;
            Type = type;
            Instance = instance;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the problem type URI (RFC 7807 <c>type</c> field).
        /// </summary>
        /// <value>
        ///     The type URI, or <see langword="null" /> when omitted.
        /// </value>
        /// =================================================================================================
        public string Type { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the short human-readable summary of the problem (RFC 7807 <c>title</c> field).
        /// </summary>
        /// <value>
        ///     The problem title.
        /// </value>
        /// =================================================================================================
        public string Title { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the HTTP status code (RFC 7807 <c>status</c> field).
        /// </summary>
        /// <value>
        ///     The HTTP status code (default: 500).
        /// </value>
        /// =================================================================================================
        public int Status { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the human-readable explanation of this specific occurrence of the problem
        ///     (RFC 7807 <c>detail</c> field).
        /// </summary>
        /// <value>
        ///     The problem detail, or <see langword="null" /> when omitted.
        /// </value>
        /// =================================================================================================
        public string Detail { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the URI reference that identifies the specific occurrence of the problem
        ///     (RFC 7807 <c>instance</c> field).
        /// </summary>
        /// <value>
        ///     The instance URI, or <see langword="null" /> when omitted.
        /// </value>
        /// =================================================================================================
        public string Instance { get; }

        /// <inheritdoc />
        public async Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = Status;
            context.Response.ContentType = ProblemJsonContentType;

            var body = BuildBody();
            var bytes = Encoding.UTF8.GetBytes(body);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Execute(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = Status;
            context.Response.ContentType = ProblemJsonContentType;

            var body = BuildBody();
            var bytes = Encoding.UTF8.GetBytes(body);
            context.Response.Body.Write(bytes, 0, bytes.Length);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds the RFC 7807 problem-details JSON body.
        /// </summary>
        /// <returns>The serialized problem-details JSON string.</returns>
        /// =================================================================================================
        private string BuildBody()
        {
            var problem = new ProblemDetailsPayload
            {
                Type = Type,
                Title = Title,
                Status = Status,
                Detail = Detail,
                Instance = Instance
            };

            return JsonSerializer.Serialize(problem, SerializerOptions);
        }
    }
}