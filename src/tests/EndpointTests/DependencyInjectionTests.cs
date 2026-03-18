// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2026-03-18 20:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-18 20:27
// ***********************************************************************
//  <copyright file="DependencyInjectionTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using EndpointHostBinder;
using EndpointHostBinder.Abstractions;
using EndpointHostBinder.Models;
using EndpointTests.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class DependencyInjectionTests
    {
        [TestMethod]
        public void RegisterEndpointHostBuilder_RegistersIEndpointHostRouter()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder();

            var provider = services.BuildServiceProvider();

            var router = provider.GetService<IEndpointHostRouter>();

            Assert.IsNotNull(router);
        }

        [TestMethod]
        public void RegisterEndpointHostBuilder_RouterIsSingleton()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder();

            var provider = services.BuildServiceProvider();

            var router1 = provider.GetService<IEndpointHostRouter>();
            var router2 = provider.GetService<IEndpointHostRouter>();

            Assert.AreSame(router1, router2, "IEndpointHostRouter should be registered as Singleton");
        }

        [TestMethod]
        public void AddHostEndpoint_RegistersHandlerAsTransient()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func");

            var provider = services.BuildServiceProvider();

            var handler1 = provider.GetService<FunctionalEndpointHandler>();
            var handler2 = provider.GetService<FunctionalEndpointHandler>();

            Assert.IsNotNull(handler1);
            Assert.AreNotSame(handler1, handler2, "Handler should be registered as Transient");
        }

        [TestMethod]
        public void AddHostEndpoint_RegistersEndpointAsSingleton()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func");

            var provider = services.BuildServiceProvider();

            var ep1 = provider.GetServices<Endpoint>().Single();
            var ep2 = provider.GetServices<Endpoint>().Single();

            Assert.AreSame(ep1, ep2, "Endpoint should be registered as Singleton");
        }

        [TestMethod]
        public void AddHostEndpoint_EndpointHasCorrectNameAndPath()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("myEndpoint", "/mypath");

            var provider = services.BuildServiceProvider();

            var ep = provider.GetServices<Endpoint>().Single();

            Assert.AreEqual("myEndpoint", ep.Name);
            Assert.AreEqual(new PathString("/mypath"), ep.Path);
            Assert.IsTrue(ep.IsActive);
        }

        [TestMethod]
        public void AddHostEndpoint_WithIsActiveFalse_EndpointRegisteredInactive()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", false);

            var provider = services.BuildServiceProvider();

            var ep = provider.GetServices<Endpoint>().Single();

            Assert.IsFalse(ep.IsActive);
        }

        [TestMethod]
        public void AddHostEndpoint_WithAllowedMethods_EndpointHasCorrectMethods()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", new[] { "GET", "POST" });

            var provider = services.BuildServiceProvider();

            var ep = provider.GetServices<Endpoint>().Single();

            Assert.IsNotNull(ep.AllowedMethods);
            CollectionAssert.AreEquivalent(new[] { "GET", "POST" }, ep.AllowedMethods.ToArray());
        }

        [TestMethod]
        public void AddHostEndpoint_WithIsActiveAndAllowedMethods_AllPropertiesCorrect()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", true, new[] { "DELETE" });

            var provider = services.BuildServiceProvider();

            var ep = provider.GetServices<Endpoint>().Single();

            Assert.IsTrue(ep.IsActive);
            Assert.IsNotNull(ep.AllowedMethods);
            CollectionAssert.AreEquivalent(new[] { "DELETE" }, ep.AllowedMethods.ToArray());
        }

        [TestMethod]
        public void AddHostEndpoint_PreBuiltEndpoint_RegisteredCorrectly()
        {
            var preBuilt = new Endpoint("pre", "/pre", typeof(FunctionalEndpointHandler), false, new[] { "PATCH" });

            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>(preBuilt);

            var provider = services.BuildServiceProvider();

            var ep = provider.GetServices<Endpoint>().Single();

            Assert.AreSame(preBuilt, ep);
            Assert.IsFalse(ep.IsActive);
            Assert.IsNotNull(ep.AllowedMethods);
        }

        [TestMethod]
        public void AddHostEndpoint_MultipleEndpoints_AllRegistered()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<EndpointOneHandler>("ep1", "/ep1")
                .AddHostEndpoint<EndpointTwoHandler>("ep2", "/ep2")
                .AddHostEndpoint<EndpointThreeHandler>("ep3", "/ep3");

            var provider = services.BuildServiceProvider();

            var endpoints = provider.GetServices<Endpoint>().ToList();

            Assert.AreEqual(3, endpoints.Count);
            Assert.IsTrue(endpoints.Any(e => e.Name == "ep1"));
            Assert.IsTrue(endpoints.Any(e => e.Name == "ep2"));
            Assert.IsTrue(endpoints.Any(e => e.Name == "ep3"));
        }

        [TestMethod]
        public void AddHostEndpoint_RouterCanResolveAllRegisteredHandlers()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func");

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>();

            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/func"), 
                    Method = "GET"
                }, 
                RequestServices = provider
            };

            var handler = router.Find(ctx);

            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(FunctionalEndpointHandler));
        }
    }
}