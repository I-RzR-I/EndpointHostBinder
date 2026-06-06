// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 17:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:07
//  ***********************************************************************
//  <copyright file="CompiledExecutorSyncPathTests.cs" company="RzR SOFT & TECH">
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
using RzR.Infrastructure.EndpointHosting.Configuration;
using RzR.Infrastructure.EndpointHosting.Execution;
using RzR.Infrastructure.EndpointHosting.Host;
using RzR.Infrastructure.EndpointHosting.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class CompiledExecutorSyncPathTests
    {
        [TestMethod]
        public async Task ExecuteAsync_OnSyncBuiltExecutor_SetsStatusWithoutNre_Test()
        {
            var executor = CompiledEndpointExecutorFactory.Create(typeof(FunctionalEndpointHandler));

            var services = new ServiceCollection()
                .AddTransient<FunctionalEndpointHandler>()
                .BuildServiceProvider();

            var ctx = new DefaultHttpContext { RequestServices = services };
            ctx.Response.Body = new MemoryStream();

            await executor.ExecuteAsync(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode,
                "Sync executor dispatched through ExecuteAsync must set the response status without NullReferenceException.");
        }

        [TestMethod]
        public void Execute_OnSyncBuiltExecutor_SetsResponseStatus_Test()
        {
            var executor = CompiledEndpointExecutorFactory.Create(typeof(FunctionalEndpointHandler));

            var services = new ServiceCollection()
                .AddTransient<FunctionalEndpointHandler>()
                .BuildServiceProvider();

            var ctx = new DefaultHttpContext { RequestServices = services };
            ctx.Response.Body = new MemoryStream();

            executor.Execute(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode,
                "Sync executor Execute must invoke the handler and set the response status.");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_OnAsyncBuiltExecutor_ThrowsInvalidOperationException_Test()
        {
            var executor = CompiledEndpointExecutorFactory.CreateTask(typeof(FunctionalEndpointHandler));

            var services = new ServiceCollection()
                .AddTransient<FunctionalEndpointHandler>()
                .BuildServiceProvider();

            var ctx = new DefaultHttpContext { RequestServices = services };
            ctx.Response.Body = new MemoryStream();

            executor.Execute(ctx);
        }

        [TestMethod]
        public async Task Middleware_InvokeAsync_WithSyncExecutor_WritesResponseCorrectly_Test()
        {
            var syncExecutor = CompiledEndpointExecutorFactory.Create(typeof(FunctionalEndpointHandler));
            var endpoint = new Endpoint("func-sync", "/func-sync", typeof(FunctionalEndpointHandler),
                true, Array.Empty<HttpMethod>(), syncExecutor);

            var router = new EndpointHostRouter(
                new[] { endpoint },
                MockLogger.Create<EndpointHostRouter>(),
                new EndpointHostOptions());

            var services = new ServiceCollection()
                .AddLogging()
                .AddTransient<FunctionalEndpointHandler>()
                .BuildServiceProvider();

            var nextWasCalled = new BoxedBool();
            var middleware = new EndpointHostBinderMiddleware(
                _ =>
                {
                    nextWasCalled.Value = true;
                    return Task.CompletedTask;
                },
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext { Request = { Path = "/func-sync", Method = "GET" }, RequestServices = services };
            ctx.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(ctx, router);

            Assert.IsFalse(nextWasCalled.Value, "Middleware should NOT call next when endpoint matches.");
            Assert.AreEqual(200, ctx.Response.StatusCode,
                "Middleware with sync executor must set the response status without NullReferenceException.");
        }
    }
}