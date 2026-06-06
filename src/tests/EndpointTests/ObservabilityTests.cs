// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 22:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:09
//  ***********************************************************************
//  <copyright file="ObservabilityTests.cs" company="RzR SOFT & TECH">
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
using RzR.Infrastructure.EndpointHosting.Diagnostics;
using RzR.Infrastructure.EndpointHosting.Discovery;
using RzR.Infrastructure.EndpointHosting.Host;
using RzR.Infrastructure.EndpointHosting.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#if NET6_0_OR_GREATER
using System.Diagnostics.Metrics;
#endif

#endregion

namespace EndpointTests
{
    [TestClass]
    public class ObservabilityTests
    {
#if NET6_0_OR_GREATER
        [TestMethod]
        public async Task Metrics_MatchedEndpoint_IncrementsRequestsMatchedCounter_Test()
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

            var (middleware, router, ctx) = BuildPipeline("/func", "GET");
            await middleware.InvokeAsync(ctx, router);

            listener.RecordObservableInstruments();

            Assert.IsTrue(counts.TryGetValue("endpoint_host.requests.matched", out var matched),
                "requests.matched counter should have been incremented");
            Assert.IsTrue(matched >= 1, $"Expected at least 1 matched, got {matched}");
        }

        [TestMethod]
        public async Task Metrics_UnmatchedRequest_IncrementsRequestsNotMatchedCounter_Test()
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

            var (middleware, router, ctx) = BuildPipeline("/no-such-path", "GET");
            await middleware.InvokeAsync(ctx, router);

            listener.RecordObservableInstruments();

            Assert.IsTrue(counts.TryGetValue("endpoint_host.requests.not_matched", out var notMatched),
                "requests.not_matched counter should have been incremented");
            Assert.IsTrue(notMatched >= 1, $"Expected at least 1 not-matched, got {notMatched}");
        }

        [TestMethod]
        public async Task Metrics_NoExecutorEndpoint_IncrementsNoExecutorCounter_Test()
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

            var endpoint = new Endpoint("noexec", "/noexec", typeof(FunctionalEndpointHandler), true);
            var router = new EndpointHostRouter(
                new[] { endpoint },
                MockLogger.Create<EndpointHostRouter>(),
                new EndpointHostOptions());

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions { PassThroughOnNoExecutor = true });

            var ctx = new DefaultHttpContext { Request = { Path = "/noexec", Method = "GET" } };
            ctx.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(ctx, router);

            listener.RecordObservableInstruments();

            Assert.IsTrue(counts.TryGetValue("endpoint_host.requests.no_executor", out var noExec),
                "requests.no_executor counter should have been incremented");
            Assert.IsTrue(noExec >= 1, $"Expected at least 1 no-executor, got {noExec}");
        }
#endif

        [TestMethod]
        public async Task Tracing_MatchedEndpoint_StartsActivityWithExpectedTags_Test()
        {
            Activity capturedActivity = null;

            using var activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == EndpointHostDiagnostics.ActivitySourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStopped = a => capturedActivity = a
            };
            ActivitySource.AddActivityListener(activityListener);

            var (middleware, router, ctx) = BuildPipeline("/func", "GET");
            await middleware.InvokeAsync(ctx, router);

            Assert.IsNotNull(capturedActivity, "An Activity should have been started for a matched endpoint");
            Assert.AreEqual("GET", GetTag(capturedActivity, "http.method") as string,
                "Activity should carry http.method tag");
            Assert.AreEqual("/func", GetTag(capturedActivity, "http.route") as string,
                "Activity should carry http.route tag matching endpoint path");
            Assert.AreEqual(true, (bool)GetTag(capturedActivity, "endpoint.matched"),
                "Activity should have endpoint.matched = true");
        }

        [TestMethod]
        public async Task Tracing_MatchedEndpoint_ActivityNameIsEndpointName_Test()
        {
            string capturedName = null;

            using var activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == EndpointHostDiagnostics.ActivitySourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStopped = a => capturedName = a.DisplayName
            };
            ActivitySource.AddActivityListener(activityListener);

            var (middleware, router, ctx) = BuildPipeline("/func", "GET");
            await middleware.InvokeAsync(ctx, router);

            Assert.IsNotNull(capturedName, "Activity display name should be set");
            Assert.IsFalse(string.IsNullOrEmpty(capturedName), "Activity display name should not be empty");
        }

        [TestMethod]
        public async Task Tracing_UnmatchedRequest_NoActivityStarted_Test()
        {
            var activitiesStarted = 0;

            using var activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == EndpointHostDiagnostics.ActivitySourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStarted = _ => activitiesStarted++
            };
            ActivitySource.AddActivityListener(activityListener);

            var (middleware, router, ctx) = BuildPipeline("/no-such-path", "GET");
            await middleware.InvokeAsync(ctx, router);

            Assert.AreEqual(0, activitiesStarted,
                "No Activity should be started when the request does not match any endpoint");
        }

        private static object GetTag(Activity activity, string key)
            => activity.TagObjects.FirstOrDefault(t => t.Key == key).Value;

        private static (EndpointHostBinderMiddleware middleware, EndpointHostRouter router, DefaultHttpContext ctx)
            BuildPipeline(string path, string method)
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func");

            var provider = services.BuildServiceProvider();
            var router = (EndpointHostRouter)provider.GetRequiredService<IEndpointHostRouter>();

            var middleware = new EndpointHostBinderMiddleware(
                _ => Task.CompletedTask,
                MockLogger.Create<EndpointHostBinderMiddleware>(),
                new EndpointHostOptions());

            var ctx = new DefaultHttpContext { Request = { Path = path, Method = method }, RequestServices = provider };
            ctx.Response.Body = new MemoryStream();

            return (middleware, router, ctx);
        }
    }
}