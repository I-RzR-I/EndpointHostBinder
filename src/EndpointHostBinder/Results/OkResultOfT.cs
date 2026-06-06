// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:01
//  ***********************************************************************
//  <copyright file="OkResultOfT.cs" company="RzR SOFT & TECH">
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
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Results
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An <see cref="IEndpointHostResult" /> that produces an HTTP 200 OK response whose body is
    ///     the JSON-serialized form of <typeparamref name="T" />, with Content-Type
    ///     <c>application/json; charset=utf-8</c>.
    /// </summary>
    /// <typeparam name="T">
    ///     The declared type of the payload. System.Text.Json serializes by this declared type, so
    ///     only members visible on <typeparamref name="T" /> are emitted. If the payload is a
    ///     subclass or implements an interface, derived members not present on
    ///     <typeparamref name="T" /> are not included in the output. To serialize by runtime type,
    ///     declare <typeparamref name="T" /> as <c>object</c> (e.g.
    ///     <c>EndpointResults.Ok&lt;object&gt;(payload)</c>). A null payload serializes to JSON
    ///     <c>null</c>.
    /// </typeparam>
    /// <seealso cref="T:RzR.Infrastructure.EndpointHosting.Abstractions.IEndpointHostResult" />
    /// =================================================================================================
    public sealed class OkResult<T> : IEndpointHostResult
    {
        private const string JsonContentType = "application/json; charset=utf-8";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="OkResult{T}" /> class.
        /// </summary>
        /// <param name="payload">The object to serialize as the response body.</param>
        /// <param name="serializerOptions">
        ///     (Optional) The <see cref="JsonSerializerOptions" /> to use. Pass <see langword="null" />
        ///     to use the System.Text.Json defaults.
        /// </param>
        /// =================================================================================================
        public OkResult(T payload, JsonSerializerOptions serializerOptions = null)
        {
            Payload = payload;
            SerializerOptions = serializerOptions;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the payload that will be serialized to JSON.
        /// </summary>
        /// <value>
        ///     The payload value.
        /// </value>
        /// =================================================================================================
        public T Payload { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the serializer options used when serializing <see cref="Payload" />.
        /// </summary>
        /// <value>
        ///     The <see cref="JsonSerializerOptions" />, or <see langword="null" /> to use defaults.
        /// </value>
        /// =================================================================================================
        public JsonSerializerOptions SerializerOptions { get; }

        /// <inheritdoc />
        public async Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = JsonContentType;

            var json = JsonSerializer.Serialize(Payload, SerializerOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Execute(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = JsonContentType;

            var json = JsonSerializer.Serialize(Payload, SerializerOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            context.Response.Body.Write(bytes, 0, bytes.Length);
        }
    }
}