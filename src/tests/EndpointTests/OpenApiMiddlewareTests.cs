// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:11
//  ***********************************************************************
//  <copyright file="OpenApiMiddlewareTests.cs" company="RzR SOFT & TECH">
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
using RzR.Infrastructure.EndpointHosting.Discovery;
using RzR.Infrastructure.EndpointHosting.OpenApi;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class OpenApiMiddlewareTests
    {
        private static DefaultHttpContext MakeContext(string path)
        {
            var ctx = new DefaultHttpContext { Request = { Path = path, Method = "GET" } };
            ctx.Response.Body = new MemoryStream();

            return ctx;
        }

        private static string ReadBody(DefaultHttpContext ctx)
        {
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);

            return new StreamReader(ctx.Response.Body, Encoding.UTF8).ReadToEnd();
        }

        [TestMethod]
        public async Task UseEndpointHostOpenApi_MatchingPath_Returns200WithApplicationJson_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<EndpointOneHandler>("ep1", "/api/users");

            var provider = services.BuildServiceProvider();
            var appBuilder = new MinimalApplicationBuilder(provider);

            appBuilder.UseEndpointHostOpenApi();
            var pipeline = appBuilder.Build();

            var ctx = MakeContext("/openapi.json");
            ctx.RequestServices = provider;

            await pipeline(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.Contains("application/json"));
        }

        [TestMethod]
        public async Task UseEndpointHostOpenApi_MatchingPath_BodyIsValidJsonWithPaths_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<EndpointOneHandler>("ep1", "/api/users");

            var provider = services.BuildServiceProvider();
            var appBuilder = new MinimalApplicationBuilder(provider);

            appBuilder.UseEndpointHostOpenApi();
            var pipeline = appBuilder.Build();

            var ctx = MakeContext("/openapi.json");
            ctx.RequestServices = provider;

            await pipeline(ctx);

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual("3.0.1", doc.RootElement.GetProperty("openapi").GetString());

            var paths = doc.RootElement.GetProperty("paths");
            Assert.IsTrue(paths.TryGetProperty("/api/users", out _), "/api/users must appear in OpenAPI paths");
        }

        [TestMethod]
        public async Task UseEndpointHostOpenApi_NonMatchingPath_PassesToNext_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<EndpointOneHandler>("ep1", "/api/users");

            var provider = services.BuildServiceProvider();
            var nextCalled = new BoxedBool();
            var appBuilder = new MinimalApplicationBuilder(provider);

            appBuilder.UseEndpointHostOpenApi();
            appBuilder.Use(_ => async ctx =>
            {
                nextCalled.Value = true;
                await Task.CompletedTask;
            });

            var pipeline = appBuilder.Build();

            var ctx = MakeContext("/other-path");
            ctx.RequestServices = provider;

            await pipeline(ctx);

            Assert.IsTrue(nextCalled.Value, "A non-matching path must pass through to the next middleware");
        }

        [TestMethod]
        public async Task UseEndpointHostOpenApi_RouterNotRegistered_Returns500_Test()
        {
            // No router registered in the service provider.
            var services = new ServiceCollection().AddLogging();
            var provider = services.BuildServiceProvider();
            var appBuilder = new MinimalApplicationBuilder(provider);

            appBuilder.UseEndpointHostOpenApi();
            var pipeline = appBuilder.Build();

            var ctx = MakeContext("/openapi.json");
            ctx.RequestServices = provider;

            await pipeline(ctx);

            Assert.AreEqual(500, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task UseEndpointHostOpenApi_DefaultPath_IsOpenApiJson_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<EndpointOneHandler>("ep1", "/api/items");

            var provider = services.BuildServiceProvider();
            var appBuilder = new MinimalApplicationBuilder(provider);

            appBuilder.UseEndpointHostOpenApi();
            var pipeline = appBuilder.Build();

            var ctx = MakeContext("/openapi.json");
            ctx.RequestServices = provider;

            await pipeline(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task UseEndpointHostOpenApi_PathMatchIsCaseInsensitive_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<EndpointOneHandler>("ep1", "/api/items");

            var provider = services.BuildServiceProvider();
            var appBuilder = new MinimalApplicationBuilder(provider);

            appBuilder.UseEndpointHostOpenApi("/Openapi.json");
            var pipeline = appBuilder.Build();

            var ctx = MakeContext("/openapi.json");
            ctx.RequestServices = provider;

            await pipeline(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
        }
    }
}