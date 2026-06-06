// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:01
//  ***********************************************************************
//  <copyright file="TextResult.cs" company="RzR SOFT & TECH">
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
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Results
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An <see cref="IEndpointHostResult" /> that writes a plain-text (or custom content-type)
    ///     string body with a configurable HTTP status code.
    /// </summary>
    /// <seealso cref="T:RzR.Infrastructure.EndpointHosting.Abstractions.IEndpointHostResult" />
    /// =================================================================================================
    public sealed class TextResult : IEndpointHostResult
    {
        private const string DefaultContentType = "text/plain; charset=utf-8";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="TextResult" /> class.
        /// </summary>
        /// <param name="text">The response body text.</param>
        /// <param name="statusCode">
        ///     (Optional) The HTTP status code. Defaults to 200.
        /// </param>
        /// <param name="contentType">
        ///     (Optional) The Content-Type header. Defaults to <c>text/plain; charset=utf-8</c>.
        /// </param>
        /// =================================================================================================
        public TextResult(string text, int statusCode = 200, string contentType = null)
        {
            Text = text ?? string.Empty;
            StatusCode = statusCode;
            ContentType = string.IsNullOrEmpty(contentType) ? DefaultContentType : contentType;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the text body that will be written to the response.
        /// </summary>
        /// <value>
        ///     The response body text.
        /// </value>
        /// =================================================================================================
        public string Text { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the HTTP status code that will be applied to the response.
        /// </summary>
        /// <value>
        ///     The HTTP status code (e.g. 200).
        /// </value>
        /// =================================================================================================
        public int StatusCode { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the Content-Type header value.
        /// </summary>
        /// <value>
        ///     The Content-Type header value (default: <c>text/plain; charset=utf-8</c>).
        /// </value>
        /// =================================================================================================
        public string ContentType { get; }

        /// <inheritdoc />
        public async Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = StatusCode;
            context.Response.ContentType = ContentType;

            var bytes = Encoding.UTF8.GetBytes(Text);
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Execute(HttpContext context)
        {
            context.ThrowIfArgNull(nameof(context));

            context.Response.StatusCode = StatusCode;
            context.Response.ContentType = ContentType;

            var bytes = Encoding.UTF8.GetBytes(Text);
            context.Response.Body.Write(bytes, 0, bytes.Length);
        }
    }
}