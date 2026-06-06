// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 05-06-2026 00:02
//  ***********************************************************************
//  <copyright file="EndpointResults.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using RzR.Infrastructure.EndpointHosting.Abstractions;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Results
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A static factory that provides concise helper methods for constructing the built-in
    ///     <see cref="IEndpointHostResult" /> implementations. Handlers can return these directly
    ///     from <c>RequestProcessAsync</c> / <c>RequestProcess</c> without instantiating result
    ///     types by hand.
    /// </summary>
    /// =================================================================================================
    public static class EndpointResults
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns an HTTP 200 OK result with an empty body.
        /// </summary>
        /// <returns>An <see cref="IEndpointHostResult" /> that produces a 200 response.</returns>
        /// =================================================================================================
        public static IEndpointHostResult Ok()
            => new OkResult();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns an HTTP 200 OK result whose body is the JSON-serialized form of
        ///     <paramref name="payload" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The declared type of the payload. Serialization uses only members visible on
        ///     <typeparamref name="T" /> (System.Text.Json default). To include derived members of a
        ///     polymorphic or interface-typed value, declare <typeparamref name="T" /> as
        ///     <c>object</c>. A null <paramref name="payload" /> serializes to JSON <c>null</c>.
        /// </typeparam>
        /// <param name="payload">The object to serialize as the response body.</param>
        /// <returns>
        ///     An <see cref="IEndpointHostResult" /> that produces a 200 JSON response.
        /// </returns>
        /// =================================================================================================
        public static IEndpointHostResult Ok<T>(T payload)
            => new OkResult<T>(payload);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a JSON result with a configurable HTTP status code.
        /// </summary>
        /// <typeparam name="T">
        ///     The declared type of the payload. Serialization uses only members visible on
        ///     <typeparamref name="T" /> (System.Text.Json default). To include derived members of a
        ///     polymorphic or interface-typed value, declare <typeparamref name="T" /> as
        ///     <c>object</c>. A null <paramref name="payload" /> serializes to JSON <c>null</c>.
        /// </typeparam>
        /// <param name="payload">The object to serialize as the response body.</param>
        /// <param name="statusCode">
        ///     (Optional) The HTTP status code. Defaults to 200.
        /// </param>
        /// <returns>
        ///     An <see cref="IEndpointHostResult" /> that produces a JSON response with the given
        ///     status code.
        /// </returns>
        /// =================================================================================================
        public static IEndpointHostResult Json<T>(T payload, int statusCode = 200)
            => new JsonResult<T>(payload, statusCode);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a plain-text (or custom content-type) result with a configurable HTTP status
        ///     code.
        /// </summary>
        /// <param name="text">The response body text.</param>
        /// <param name="statusCode">
        ///     (Optional) The HTTP status code. Defaults to 200.
        /// </param>
        /// <param name="contentType">
        ///     (Optional) The Content-Type header value. Defaults to <c>text/plain; charset=utf-8</c>.
        /// </param>
        /// <returns>
        ///     An <see cref="IEndpointHostResult" /> that writes the given text with the given
        ///     content type.
        /// </returns>
        /// =================================================================================================
        public static IEndpointHostResult Text(string text, int statusCode = 200, string contentType = null)
            => new TextResult(text, statusCode, contentType);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns an HTTP 204 No Content result with an empty body.
        /// </summary>
        /// <returns>An <see cref="IEndpointHostResult" /> that produces a 204 response.</returns>
        /// =================================================================================================
        public static IEndpointHostResult NoContent()
            => new NoContentResult();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns an HTTP 404 Not Found result with an empty body.
        /// </summary>
        /// <returns>An <see cref="IEndpointHostResult" /> that produces a 404 response.</returns>
        /// =================================================================================================
        public static IEndpointHostResult NotFound()
            => new NotFoundResult();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a result that sets the HTTP response status code and writes no body.
        /// </summary>
        /// <param name="code">The HTTP status code to set on the response.</param>
        /// <returns>
        ///     An <see cref="IEndpointHostResult" /> that sets the given status code.
        /// </returns>
        /// =================================================================================================
        public static IEndpointHostResult StatusCode(int code)
            => new StatusCodeResult(code);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns an RFC 7807 Problem Details result (<c>application/problem+json</c>) with a
        ///     configurable status code, title, detail, type, and instance.
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
        /// <returns>
        ///     An <see cref="IEndpointHostResult" /> that writes an RFC 7807 problem-details body.
        /// </returns>
        /// =================================================================================================
        public static IEndpointHostResult Problem(string title, string detail = null, int statusCode = 500,
            string type = null, string instance = null)
            => new ProblemDetailsResult(title, detail, statusCode, type, instance);
    }
}