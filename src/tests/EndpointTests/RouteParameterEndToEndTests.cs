// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:13
//  ***********************************************************************
//  <copyright file="RouteParameterEndToEndTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class RouteParameterEndToEndTests
    {
        private static (EndpointHostBinderMiddleware middleware, EndpointHostRouter router)
            BuildPipeline<THandler>(string template, out IServiceProvider provider)
            where THandler : class, IEndpointHostRequestHandler
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<THandler>("ep", template);

            provider = services.BuildServiceProvider();
            var router = (EndpointHostRouter)provider.GetRequiredService<IEndpointHostRouter>();

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            return (middleware, router);
        }

        private static DefaultHttpContext MakeContext(string path, IServiceProvider provider)
        {
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString(path), 
                    Method = "GET"
                }, RequestServices = provider
            };
            ctx.Response.Body = new MemoryStream();

            return ctx;
        }

        private static string ReadBody(DefaultHttpContext ctx)
        {
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);

            return new StreamReader(ctx.Response.Body, Encoding.UTF8).ReadToEnd();
        }

        [TestMethod]
        public async Task SingleSegmentParam_IntValue_CapturedAndEchoedInBody_Test()
        {
            var (middleware, router) = BuildPipeline<RouteValueEchoHandler>("/users/{id}", out var provider);
            var ctx = MakeContext("/users/42", provider);

            await middleware.InvokeAsync(ctx, router);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.AreEqual("42", ReadBody(ctx));
        }

        [TestMethod]
        public async Task SingleSegmentParam_GuidValue_CapturedAndEchoedInBody_Test()
        {
            const string guidValue = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
            var (middleware, router) = BuildPipeline<RouteValueEchoHandler>("/users/{id}", out var provider);
            var ctx = MakeContext("/users/" + guidValue, provider);

            await middleware.InvokeAsync(ctx, router);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.AreEqual(guidValue, ReadBody(ctx));
        }

        [TestMethod]
        public async Task MultiSegment_OrdidAndItemId_BothCapturedAndReturnedAsJson_Test()
        {
            const string orderId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
            const string itemId = "99";

            var (middleware, router) = BuildPipeline<MultiSegmentRouteEchoHandler>(
                "/orders/{orderId}/items/{itemId}", out var provider);
            var ctx = MakeContext("/orders/" + orderId + "/items/" + itemId, provider);

            await middleware.InvokeAsync(ctx, router);

            Assert.AreEqual(200, ctx.Response.StatusCode);

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual(orderId, doc.RootElement.GetProperty("orderId").GetString());
            Assert.AreEqual(itemId, doc.RootElement.GetProperty("itemId").GetString());
        }

        [TestMethod]
        public async Task GetEndpointRouteValues_ViaDictionary_ContainsAllCapturedParams_Test()
        {
            // Verify that context.GetEndpointRouteValues() in the handler receives all values.
            var captured = (IReadOnlyDictionary<string, string>)null;

            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<RouteCapturingHandler>("ep", "/items/{id}");

            services.AddTransient(_ =>
                new RouteCapturingHandler(ctx =>
                    captured = ctx.GetEndpointRouteValues()));

            var provider = services.BuildServiceProvider();
            var router = (EndpointHostRouter)provider.GetRequiredService<IEndpointHostRouter>();

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = MakeContext("/items/777", provider);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsNotNull(captured, "Handler should have been invoked");
            Assert.IsTrue(captured.ContainsKey("id"));
            Assert.AreEqual("777", captured["id"]);
        }
    }
}