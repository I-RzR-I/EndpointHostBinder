// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:06
//  ***********************************************************************
//  <copyright file="MinimalApplicationBuilder.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace EndpointTests.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A minimal IApplicationBuilder used to drive UseEndpointHostOpenApi in tests without
    ///     requiring a full ASP.NET Core host.
    /// </summary>
    /// =================================================================================================
    internal sealed class MinimalApplicationBuilder : IApplicationBuilder
    {
        private readonly List<Func<RequestDelegate, RequestDelegate>> _middlewares = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="MinimalApplicationBuilder" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to expose to middleware.</param>
        /// =================================================================================================
        public MinimalApplicationBuilder(IServiceProvider serviceProvider) => ApplicationServices = serviceProvider;

        /// <inheritdoc />
        public IServiceProvider ApplicationServices { get; set; }

        /// <inheritdoc />
        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public IFeatureCollection ServerFeatures { get; } = new FeatureCollection();

        /// <inheritdoc />
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            _middlewares.Add(middleware);

            return this;
        }

        /// <inheritdoc />
        public IApplicationBuilder New() => new MinimalApplicationBuilder(ApplicationServices);

        /// <inheritdoc />
        public RequestDelegate Build()
        {
            RequestDelegate app = _ => Task.CompletedTask;
            for (var i = _middlewares.Count - 1; i >= 0; i--)
                app = _middlewares[i](app);

            return app;
        }
    }
}