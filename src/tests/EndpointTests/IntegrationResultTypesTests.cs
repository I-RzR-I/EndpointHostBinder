// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:09
//  ***********************************************************************
//  <copyright file="IntegrationResultTypesTests.cs" company="RzR SOFT & TECH">
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
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class IntegrationResultTypesTests
    {
        private static (EndpointHostBinderMiddleware middleware, EndpointHostRouter router, DefaultHttpContext ctx)
            BuildPipeline<THandler>(string path, out BoxedBool nextCalled)
            where THandler : class, IEndpointHostRequestHandler
        {
            var box = new BoxedBool();
            nextCalled = box;

            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<THandler>("ep", path);

            var provider = services.BuildServiceProvider();
            var router = (EndpointHostRouter)provider.GetRequiredService<IEndpointHostRouter>();

            var middleware = new EndpointHostBinderMiddleware(
                _ =>
                {
                    box.Value = true;
                    return Task.CompletedTask;
                },
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext { Request = { Path = path, Method = "GET" }, RequestServices = provider };
            ctx.Response.Body = new MemoryStream();

            return (middleware, router, ctx);
        }

        private static string ReadBody(DefaultHttpContext ctx)
        {
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);

            return new StreamReader(ctx.Response.Body, Encoding.UTF8).ReadToEnd();
        }

        [TestMethod]
        public async Task OkJson_MiddlewareThroughHandler_Returns200WithJsonBody_Test()
        {
            var (middleware, router, ctx) = BuildPipeline<OkJsonHandler>("/ok-json", out var nextCalled);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsFalse(nextCalled.Value, "Next must NOT be called on a matched endpoint");
            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.Contains("application/json"));

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual("Alice", doc.RootElement.GetProperty("Name").GetString());
            Assert.AreEqual(30, doc.RootElement.GetProperty("Age").GetInt32());
        }

        [TestMethod]
        public async Task NotFound_MiddlewareThroughHandler_Returns404_Test()
        {
            var (middleware, router, ctx) = BuildPipeline<NotFoundHandler>("/not-found-ep", out var nextCalled);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsFalse(nextCalled.Value, "Next must NOT be called on a matched endpoint");
            Assert.AreEqual(404, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task NoContent_MiddlewareThroughHandler_Returns204_Test()
        {
            var (middleware, router, ctx) = BuildPipeline<NoContentHandler>("/no-content-ep", out var nextCalled);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsFalse(nextCalled.Value, "Next must NOT be called on a matched endpoint");
            Assert.AreEqual(204, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task Text_MiddlewareThroughHandler_Returns200TextPlainWithExactBody_Test()
        {
            var (middleware, router, ctx) = BuildPipeline<TextHandler>("/text-ep", out var nextCalled);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsFalse(nextCalled.Value, "Next must NOT be called on a matched endpoint");
            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.StartsWith("text/plain"));

            var body = ReadBody(ctx);
            Assert.AreEqual(TextHandler.Body, body);
        }

        [TestMethod]
        public async Task Problem_MiddlewareThroughHandler_ReturnsProblemJson_Test()
        {
            var (middleware, router, ctx) = BuildPipeline<ProblemHandler>("/problem-ep", out var nextCalled);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsFalse(nextCalled.Value, "Next must NOT be called on a matched endpoint");
            Assert.AreEqual(422, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.StartsWith("application/problem+json"));

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual(ProblemHandler.ProblemTitle, doc.RootElement.GetProperty("title").GetString());
            Assert.AreEqual(422, doc.RootElement.GetProperty("status").GetInt32());
        }

        [TestMethod]
        public async Task StatusCode_MiddlewareThroughHandler_Returns418_Test()
        {
            var (middleware, router, ctx) = BuildPipeline<StatusCodeHandler>("/status-code-ep", out var nextCalled);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsFalse(nextCalled.Value, "Next must NOT be called on a matched endpoint");
            Assert.AreEqual(StatusCodeHandler.Code, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task UnmatchedPath_MiddlewareCallsNext_Test()
        {
            var (middleware, router, ctx) = BuildPipeline<FunctionalEndpointHandler>("/functional-ep", out var nextCalled);
            ctx.Request.Path = "/completely-different";

            await middleware.InvokeAsync(ctx, router);

            Assert.IsTrue(nextCalled.Value, "Next MUST be called when no endpoint matches");
        }
    }
}