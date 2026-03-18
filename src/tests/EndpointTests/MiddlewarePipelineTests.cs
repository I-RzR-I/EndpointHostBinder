// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2026-03-18 20:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-18 20:32
// ***********************************************************************
//  <copyright file="MiddlewarePipelineTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using EndpointHostBinder;
using EndpointHostBinder.Abstractions;
using EndpointHostBinder.Host;
using EndpointHostBinder.Models;
using EndpointTests.Common;
using EndpointTests.Handlers;
using EndpointTests.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class MiddlewarePipelineTests
    {
        [TestMethod]
        public async Task Invoke_MatchedEndpoint_WritesResponseAndShortCircuits_Test()
        {
            var (middleware, router, ctx) = BuildPipeline("/func", "GET",
                out var nextWasCalled,
                true,
                true);

            await middleware.Invoke(ctx, router);

            Assert.IsFalse(nextWasCalled.Value, "Next middleware should NOT be called when endpoint matches");
            Assert.AreEqual(200, ctx.Response.StatusCode);

            var body = ReadResponseBody(ctx);
            Assert.AreEqual(FunctionalEndpointResult.ResponseBody, body);
        }

        [TestMethod]
        public async Task Invoke_UnmatchedPath_CallsNextMiddleware_Test()
        {
            var (middleware, router, ctx) = BuildPipeline("/unknown", "GET",
                out var nextWasCalled,
                true,
                true);

            await middleware.Invoke(ctx, router);

            Assert.IsTrue(nextWasCalled.Value, "Next middleware should be called when no endpoint matches");
        }

        [TestMethod]
        public async Task Invoke_DisabledEndpoint_CallsNextMiddleware_Test()
        {
            var (middleware, router, ctx) = BuildPipeline("/func", "GET",
                out var nextWasCalled,
                true,
                false); // <-- inactive

            await middleware.Invoke(ctx, router);

            Assert.IsTrue(nextWasCalled.Value, "Next middleware should be called when matched endpoint is disabled");
        }

        [TestMethod]
        public async Task Invoke_MethodNotAllowed_CallsNextMiddleware_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", new[] { HttpMethod.Get });

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>() as EndpointHostRouter;

            var nextWasCalled = new BoxedBool();
            var middleware = new EndpointHostBinderMiddleware(
                _ =>
                {
                    nextWasCalled.Value = true;
                    return Task.CompletedTask;
                },
                MockLogger.Create<EndpointHostBinderMiddleware>());

            var ctx = new DefaultHttpContext { Request = { Path = "/func", Method = "POST" }, RequestServices = provider };
            AttachResponseBody(ctx);

            await middleware.Invoke(ctx, router);

            Assert.IsTrue(nextWasCalled.Value, "POST to GET-only endpoint should fall through to next");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Invoke_HandlerThrows_ExceptionBubblesUp_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<ThrowingEndpointHandler>("throw", "/throw");

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>() as EndpointHostRouter;

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>());

            var ctx = new DefaultHttpContext { Request = { Path = "/throw", Method = "GET" }, RequestServices = provider };

            await middleware.Invoke(ctx, router);
        }

        [TestMethod]
        public async Task Invoke_CancellationTokenForwarded_ToHandlerAndResult_Test()
        {
            var cts = new CancellationTokenSource();
            var receivedToken = CancellationToken.None;

            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder();

            // Register a capturing handler directly through the pre-built endpoint overload
            services.AddTransient(sp => new TokenCapturingHandler(t => receivedToken = t));
            services.AddSingleton(new Endpoint(
                "capture", "/capture", typeof(TokenCapturingHandler)));

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>() as EndpointHostRouter;

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>());

            var ctx = new DefaultHttpContext { Request = { Path = "/capture", Method = "GET" }, RequestServices = provider };
            // assign RequestAborted to our source's token
            ctx.RequestAborted = cts.Token;
            AttachResponseBody(ctx);

            await middleware.Invoke(ctx, router);

            Assert.AreEqual(cts.Token, receivedToken, "RequestAborted CancellationToken should be forwarded to the handler");
        }

        [TestMethod]
        public async Task Invoke_MultipleSequentialRequests_AllHandledCorrectly_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func")
                .AddHostEndpoint<EndpointOneHandler>("ep1", "/ep1");

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>() as EndpointHostRouter;

            for (var i = 0; i < 5; i++)
            {
                var nextWasCalled = new BoxedBool();
                var middleware = new EndpointHostBinderMiddleware(
                    _ =>
                    {
                        nextWasCalled.Value = true;
                        return Task.CompletedTask;
                    },
                    MockLogger.Create<EndpointHostBinderMiddleware>());

                var ctx = new DefaultHttpContext { Request = { Path = "/func", Method = "GET" }, RequestServices = provider };
                AttachResponseBody(ctx);

                await middleware.Invoke(ctx, router);

                Assert.IsFalse(nextWasCalled.Value, $"Iteration {i}: next should not be called for /func");
                Assert.AreEqual(200, ctx.Response.StatusCode, $"Iteration {i}: expected 200");
            }
        }

        private static (EndpointHostBinderMiddleware middleware, EndpointHostRouter router, DefaultHttpContext ctx)
            BuildPipeline(string path, string method, out BoxedBool nextCalled, bool registerFunctional, bool active)
        {
            var box = new BoxedBool();
            nextCalled = box;

            var services = new ServiceCollection().AddLogging().RegisterEndpointHostBuilder();
            if (registerFunctional)
                services.AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", active);

            var provider = services.BuildServiceProvider();
            var router = (EndpointHostRouter)provider.GetRequiredService<IEndpointHostRouter>();

            var middleware = new EndpointHostBinderMiddleware(
                _ =>
                {
                    box.Value = true;
                    return Task.CompletedTask;
                },
                MockLogger.Create<EndpointHostBinderMiddleware>());

            var ctx = new DefaultHttpContext { Request = { Path = path, Method = method }, RequestServices = provider };
            AttachResponseBody(ctx);

            return (middleware, router, ctx);
        }

        private static void AttachResponseBody(DefaultHttpContext ctx)
            => ctx.Response.Body = new MemoryStream();

        private static string ReadResponseBody(DefaultHttpContext ctx)
        {
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);
            return new StreamReader(ctx.Response.Body, Encoding.UTF8).ReadToEnd();
        }

        private class BoxedBool
        {
            public bool Value { get; set; }
        }

        // Handler that records the CancellationToken it receives
        private class TokenCapturingHandler : IEndpointHostRequestHandler
        {
            private readonly Action<CancellationToken> _capture;

            public TokenCapturingHandler(Action<CancellationToken> capture) => _capture = capture;

            public Task<IEndpointHostResult> RequestProcessAsync(
                HttpContext context, CancellationToken cancellationToken = default)
            {
                _capture(cancellationToken);
                return Task.FromResult<IEndpointHostResult>(new FunctionalEndpointResult());
            }

            public IEndpointHostResult RequestProcess(HttpContext context)
                => new FunctionalEndpointResult();
        }

        // Handler that always throws — used to verify exception propagation
        private class ThrowingEndpointHandler : IEndpointHostRequestHandler
        {
            public Task<IEndpointHostResult> RequestProcessAsync(
                HttpContext context, CancellationToken cancellationToken = default)
                => throw new InvalidOperationException("Handler failure");

            public IEndpointHostResult RequestProcess(HttpContext context)
                => throw new InvalidOperationException("Handler failure");
        }
    }
}