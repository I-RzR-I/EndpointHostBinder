// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:10
//  ***********************************************************************
//  <copyright file="MetricsDeltaTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.VisualStudio.TestTools.UnitTesting;
#if NET6_0_OR_GREATER
using System.Diagnostics.Metrics;
#endif

#endregion

namespace EndpointTests
{
    [TestClass]
    public class MetricsDeltaTests
    {
#if NET6_0_OR_GREATER
        private static (EndpointHostBinderMiddleware middleware, EndpointHostRouter router, DefaultHttpContext ctx)
            BuildMatchedPipeline()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func-delta");

            var provider = services.BuildServiceProvider();
            var router = (EndpointHostRouter)provider.GetRequiredService<IEndpointHostRouter>();

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext
            {
                Request = { Path = "/func-delta", Method = "GET" },
                RequestServices = provider
            };
            ctx.Response.Body = new MemoryStream();

            return (middleware, router, ctx);
        }

        private static Dictionary<string, long> CaptureSingleRequest(
            Func<Task> invokeRequest)
        {
            var counts = new Dictionary<string, long>(StringComparer.Ordinal);

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == EndpointHostDiagnostics.MeterName)
                    l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
            {
                counts.TryGetValue(instrument.Name, out var existing);
                counts[instrument.Name] = existing + measurement;
            });
            listener.Start();

            invokeRequest().GetAwaiter().GetResult();

            listener.RecordObservableInstruments();

            return counts;
        }

        [TestMethod]
        public async Task Matched_ExactlyOneDeltaOnRequestsMatchedCounter_Test()
        {
            var counts = new Dictionary<string, long>(StringComparer.Ordinal);

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == EndpointHostDiagnostics.MeterName)
                    l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
            {
                counts.TryGetValue(instrument.Name, out var existing);
                counts[instrument.Name] = existing + measurement;
            });
            listener.Start();

            var (middleware, router, ctx) = BuildMatchedPipeline();
            await middleware.InvokeAsync(ctx, router);

            listener.RecordObservableInstruments();

            Assert.IsTrue(counts.TryGetValue("endpoint_host.requests.matched", out var delta),
                "endpoint_host.requests.matched counter must be incremented");
            Assert.AreEqual(1L, delta, "Exactly one matched request must produce delta = 1");
        }

        [TestMethod]
        public async Task NotMatched_ExactlyOneDeltaOnRequestsNotMatchedCounter_Test()
        {
            var counts = new Dictionary<string, long>(StringComparer.Ordinal);

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == EndpointHostDiagnostics.MeterName)
                    l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
            {
                counts.TryGetValue(instrument.Name, out var existing);
                counts[instrument.Name] = existing + measurement;
            });
            listener.Start();

            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func-nm");

            var provider = services.BuildServiceProvider();
            var router = (EndpointHostRouter)provider.GetRequiredService<IEndpointHostRouter>();

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext
            {
                Request = { Path = "/no-such-path", Method = "GET" },
                RequestServices = provider
            };
            ctx.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(ctx, router);

            listener.RecordObservableInstruments();

            Assert.IsTrue(counts.TryGetValue("endpoint_host.requests.not_matched", out var delta),
                "endpoint_host.requests.not_matched counter must be incremented");
            Assert.AreEqual(1L, delta, "Exactly one unmatched request must produce delta = 1");
        }

        [TestMethod]
        public async Task NoExecutor_ExactlyOneDeltaOnNoExecutorCounter_Test()
        {
            var counts = new Dictionary<string, long>(StringComparer.Ordinal);

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == EndpointHostDiagnostics.MeterName)
                    l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
            {
                counts.TryGetValue(instrument.Name, out var existing);
                counts[instrument.Name] = existing + measurement;
            });
            listener.Start();

            var endpoint = new Endpoint("noexec-delta", "/noexec-delta", typeof(FunctionalEndpointHandler), true);
            var router = new EndpointHostRouter(
                new[] { endpoint },
                MockLogger.Create<EndpointHostRouter>(),
                new EndpointHostOptions());

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions { PassThroughOnNoExecutor = true });

            var ctx = new DefaultHttpContext
            {
                Request = { Path = "/noexec-delta", Method = "GET" }
            };
            ctx.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(ctx, router);

            listener.RecordObservableInstruments();

            Assert.IsTrue(counts.TryGetValue("endpoint_host.requests.no_executor", out var delta),
                "endpoint_host.requests.no_executor counter must be incremented");
            Assert.AreEqual(1L, delta, "Exactly one no-executor request must produce delta = 1");
        }

#endif
    }
}