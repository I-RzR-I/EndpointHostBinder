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

using EndpointTests.Handlers;
using EndpointTests.Helpers;
using EndpointTests.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Configuration;
using RzR.Infrastructure.EndpointHosting.Discovery;
using RzR.Infrastructure.EndpointHosting.Host;
using RzR.Infrastructure.EndpointHosting.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
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

            await middleware.InvokeAsync(ctx, router);

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

            await middleware.InvokeAsync(ctx, router);

            Assert.IsTrue(nextWasCalled.Value, "Next middleware should be called when no endpoint matches");
        }

        [TestMethod]
        public async Task Invoke_DisabledEndpoint_CallsNextMiddleware_Test()
        {
            var (middleware, router, ctx) = BuildPipeline("/func", "GET",
                out var nextWasCalled,
                true,
                false); // <-- inactive

            await middleware.InvokeAsync(ctx, router);

            Assert.IsTrue(nextWasCalled.Value, "Next middleware should be called when matched endpoint is disabled");
        }

        [TestMethod]
        public async Task Invoke_MethodNotAllowed_CallsNextMiddleware_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly())
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
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext { Request = { Path = "/func", Method = "POST" }, RequestServices = provider };
            AttachResponseBody(ctx);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsTrue(nextWasCalled.Value, "POST to GET-only endpoint should fall through to next");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Invoke_HandlerThrows_ExceptionBubblesUp_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly())
                .AddHostEndpoint<ThrowingEndpointHandler>("throw", "/throw");

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>() as EndpointHostRouter;

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext { Request = { Path = "/throw", Method = "GET" }, RequestServices = provider };

            await middleware.InvokeAsync(ctx, router);
        }

        [TestMethod]
        public async Task Invoke_CancellationTokenForwarded_ToHandlerAndResult_Test()
        {
            var cts = new CancellationTokenSource();
            var receivedToken = CancellationToken.None;

            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly())
                .AddHostEndpoint<TokenCapturingHandler>("capture", "/capture");

            // Override with capturing factory — last registration wins for GetRequiredService<T>
            services.AddTransient<TokenCapturingHandler>(_ => new TokenCapturingHandler(t => receivedToken = t));

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>() as EndpointHostRouter;

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext { Request = { Path = "/capture", Method = "GET" }, RequestServices = provider };
            // assign RequestAborted to our source's token
            ctx.RequestAborted = cts.Token;
            AttachResponseBody(ctx);

            await middleware.InvokeAsync(ctx, router);

            Assert.AreEqual(cts.Token, receivedToken, "RequestAborted CancellationToken should be forwarded to the handler");
        }

        [TestMethod]
        public async Task Invoke_MultipleSequentialRequests_AllHandledCorrectly_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly())
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
                    MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

                var ctx = new DefaultHttpContext { Request = { Path = "/func", Method = "GET" }, RequestServices = provider };
                AttachResponseBody(ctx);

                await middleware.InvokeAsync(ctx, router);

                Assert.IsFalse(nextWasCalled.Value, $"Iteration {i}: next should not be called for /func");
                Assert.AreEqual(200, ctx.Response.StatusCode, $"Iteration {i}: expected 200");
            }
        }

        [TestMethod]
        public async Task Invoke_NoExecutor_PassThroughTrue_CallsNextMiddleware_Test()
        {
            // Endpoint matches the path but has a null executor; default options pass through to next.
            var router = BuildNullExecutorRouter();
            var nextWasCalled = new BoxedBool();
            var middleware = new EndpointHostBinderMiddleware(
                _ =>
                {
                    nextWasCalled.Value = true;
                    return Task.CompletedTask;
                },
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions { PassThroughOnNoExecutor = true });

            var ctx = new DefaultHttpContext { Request = { Path = "/noexec", Method = "GET" } };
            AttachResponseBody(ctx);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsTrue(nextWasCalled.Value, "PassThroughOnNoExecutor=true should forward to next when executor is null");
        }

        [TestMethod]
        public async Task Invoke_NoExecutor_PassThroughFalse_ShortCircuitsWith500_Test()
        {
            // Endpoint matches the path but has a null executor; options short-circuit with a 500.
            var router = BuildNullExecutorRouter();
            var nextWasCalled = new BoxedBool();
            var middleware = new EndpointHostBinderMiddleware(
                _ =>
                {
                    nextWasCalled.Value = true;
                    return Task.CompletedTask;
                },
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions { PassThroughOnNoExecutor = false });

            var ctx = new DefaultHttpContext { Request = { Path = "/noexec", Method = "GET" } };
            AttachResponseBody(ctx);

            await middleware.InvokeAsync(ctx, router);

            Assert.IsFalse(nextWasCalled.Value, "PassThroughOnNoExecutor=false should NOT forward to next");
            Assert.AreEqual(500, ctx.Response.StatusCode, "PassThroughOnNoExecutor=false should short-circuit with 500");
        }

        private static EndpointHostRouter BuildNullExecutorRouter()
        {
            // 4-arg ctor leaves Executor null while keeping the endpoint active and matchable.
            var endpoint = new Endpoint("noexec", "/noexec", typeof(FunctionalEndpointHandler), true);

            return new EndpointHostRouter(
                new[] { endpoint },
                MockLogger.Create<EndpointHostRouter>(),
                new EndpointHostOptions());
        }

        private static (EndpointHostBinderMiddleware middleware, EndpointHostRouter router, DefaultHttpContext ctx)
            BuildPipeline(string path, string method, out BoxedBool nextCalled, bool registerFunctional, bool active)
        {
            var box = new BoxedBool();
            nextCalled = box;

            var services = new ServiceCollection().AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly());
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
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

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
    }
}
