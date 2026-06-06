// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointHostBinder
//  Author            : RzR
//  Created           : 04-06-2026 20:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:55
//  ***********************************************************************
//  <copyright file="OpenApiApplicationBuilderExtensions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RzR.Extensions.Domain.Primitives;
using RzR.Extensions.Domain.Validation;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using System;
using System.Text;

#endregion

namespace RzR.Infrastructure.EndpointHosting.OpenApi
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Provides an opt-in <see cref="IApplicationBuilder" /> extension that mounts a terminal
    ///     middleware branch serving the OpenAPI 3.0.x JSON document produced by
    ///     <see cref="OpenApiDocumentBuilder" />.
    /// </summary>
    /// <remarks>
    ///     This extension is intentionally <b>not</b> wired into <c>UseEndpointHostBuilder</c>.
    ///     Call it explicitly in your application's middleware pipeline, typically before
    ///     <c>UseEndpointHostBuilder</c>.
    /// </remarks>
    /// =================================================================================================
    public static class OpenApiApplicationBuilderExtensions
    {
        private const string JsonContentType = "application/json; charset=utf-8";

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Adds a terminal middleware branch that serves the OpenAPI 3.0.x JSON document at the
        ///     specified <paramref name="path" />. The document is built on every request from the
        ///     <see cref="IEndpointHostRouter" /> registered in the request's service provider.
        /// </summary>
        /// <remarks>
        ///     This method is opt-in and must be called explicitly. It does not modify any other
        ///     middleware, does not add package dependencies, and works across all supported TFMs.
        /// </remarks>
        /// <param name="app">The <see cref="IApplicationBuilder" /> to configure.</param>
        /// <param name="path">
        ///     (Optional) The URL path at which the OpenAPI document will be served.
        ///     Defaults to <c>/openapi.json</c>.
        /// </param>
        /// <param name="title">
        ///     (Optional) The API title placed in the document's <c>info.title</c> field.
        ///     Defaults to <c>EndpointHostBinder API</c>.
        /// </param>
        /// <param name="version">
        ///     (Optional) The API version placed in the document's <c>info.version</c> field.
        ///     Defaults to <c>1.0.0</c>.
        /// </param>
        /// <returns>
        ///     The same <see cref="IApplicationBuilder" /> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IApplicationBuilder UseEndpointHostOpenApi(this IApplicationBuilder app, string path = "/openapi.json",
            string title = OpenApiDocumentBuilder.DefaultTitle, string version = OpenApiDocumentBuilder.DefaultVersion)
        {
            app.ThrowIfArgNull(nameof(app));

            var openApiPath = string.IsNullOrEmpty(path) ? "/openapi.json" : path;

            app.Use(async (context, next) =>
            {
                if (!string.Equals(context.Request.Path.Value, openApiPath, StringComparison.OrdinalIgnoreCase))
                {
                    await next().ConfigureAwait(false);

                    return;
                }

                var router = context.RequestServices.GetService<IEndpointHostRouter>();
                if (router.IsNull())
                {
                    context.Response.StatusCode = 500;

                    return;
                }

                var builder = new OpenApiDocumentBuilder(router, title, version);
                var json = builder.BuildJson();
                var bytes = Encoding.UTF8.GetBytes(json);

                context.Response.StatusCode = 200;
                context.Response.ContentType = JsonContentType;
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length, context.RequestAborted)
                    .ConfigureAwait(false);
            });

            return app;
        }
    }
}