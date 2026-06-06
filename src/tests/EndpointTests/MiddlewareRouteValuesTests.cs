// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 21:06
//
//  Last Modified By  : RzR
//  Last Modified On  : 04-06-2026 23:12
//  ***********************************************************************
//  <copyright file="MiddlewareRouteValuesTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S A G E S

using EndpointTests.Handlers;
using EndpointTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Configuration;
using RzR.Infrastructure.EndpointHosting.Discovery;
using RzR.Infrastructure.EndpointHosting.Host;
using RzR.Infrastructure.EndpointHosting.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class MiddlewareRouteValuesTests
    {
        [TestMethod]
        public async Task Middleware_TemplatedMatch_RouteValuesAvailableInContext_Test()
        {
            IReadOnlyDictionary<string, string> captured = null;

            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<RouteCapturingHandler>("user-ep", "/users/{id}");

            services.AddTransient(_ =>
                new RouteCapturingHandler(ctx => captured = ctx.GetEndpointRouteValues()));

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>()
                as EndpointHostRouter;

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext { Request = { Path = "/users/42", Method = "GET" }, RequestServices = provider };
            ctx.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(ctx, router);

            Assert.IsNotNull(captured, "Handler should have been invoked");
            Assert.IsTrue(captured.ContainsKey("id"), "Route value 'id' must be present");
            Assert.AreEqual("42", captured["id"]);
        }

        [TestMethod]
        public async Task Middleware_LiteralMatch_EmptyRouteValues_Test()
        {
            IReadOnlyDictionary<string, string> captured = null;

            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<RouteCapturingHandler>("literal-ep", "/items");

            services.AddTransient(_ =>
                new RouteCapturingHandler(ctx => captured = ctx.GetEndpointRouteValues()));

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>()
                as EndpointHostRouter;

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext { Request = { Path = "/items", Method = "GET" }, RequestServices = provider };
            ctx.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(ctx, router);

            Assert.IsNotNull(captured, "Handler should have been invoked");
            Assert.AreEqual(0, captured.Count,
                "Literal match must not produce any route-value entries");
        }
    }
}
