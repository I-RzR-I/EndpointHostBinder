// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 21:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:07
//  ***********************************************************************
//  <copyright file="HandlerLifetimeTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using EndpointTests.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Infrastructure.EndpointHosting.Discovery;
using RzR.Infrastructure.EndpointHosting.Models;
using System.Linq;
using System.Net.Http;
using System.Reflection;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class HandlerLifetimeTests
    {
        [TestMethod]
        public void AddHostEndpoint_DefaultOverload_HandlerIsTransient_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func");

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_ExplicitTransient_HandlerIsTransient_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", ServiceLifetime.Transient);

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_ExplicitScoped_HandlerIsScoped_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", ServiceLifetime.Scoped);

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_ExplicitSingleton_HandlerIsSingleton_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", ServiceLifetime.Singleton);

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_WithIsActive_DefaultLifetimeIsTransient_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", false);

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_WithIsActiveAndScopedLifetime_HandlerIsScoped_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", true, ServiceLifetime.Scoped);

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_WithAllowedMethods_DefaultLifetimeIsTransient_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", new[] { HttpMethod.Get });

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_WithAllowedMethodsAndSingletonLifetime_HandlerIsSingleton_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", new[] { HttpMethod.Get },
                    ServiceLifetime.Singleton);

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_WithIsActiveAndAllowedMethods_DefaultLifetimeIsTransient_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", true, new[] { HttpMethod.Post });

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_WithIsActiveAndAllowedMethodsAndScopedLifetime_HandlerIsScoped_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", true, new[] { HttpMethod.Post },
                    ServiceLifetime.Scoped);

            var descriptor = services.Single(sd => sd.ServiceType == typeof(FunctionalEndpointHandler));

            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpointsFromAssembly_Default_IsCallableAndDoesNotThrow_Test()
        {
            // The executing test assembly contains no [EndpointHost]-decorated handlers,
            // so this verifies the default (Transient) overload is callable without error.
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder();

            services.AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly());

            // No endpoint descriptors are expected — the call should succeed silently.
            var endpoints = services.Where(sd =>
                sd.ServiceType == typeof(Endpoint)).ToList();
            Assert.AreEqual(0, endpoints.Count);
        }

        [TestMethod]
        public void AddHostEndpointsFromAssembly_WithScopedLifetime_IsCallableAndDoesNotThrow_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder();

            services.AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Scoped);

            // No endpoint descriptors expected — verifies the overload accepts Scoped without error.
            var endpoints = services.Where(sd =>
                sd.ServiceType == typeof(Endpoint)).ToList();
            Assert.AreEqual(0, endpoints.Count);
        }

        [TestMethod]
        public void AddHostEndpointsFromAssembly_WithSingletonLifetime_IsCallableAndDoesNotThrow_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder();

            services.AddHostEndpointsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

            // No endpoint descriptors expected — verifies the overload accepts Singleton without error.
            var endpoints = services.Where(sd =>
                sd.ServiceType == typeof(Endpoint)).ToList();
            Assert.AreEqual(0, endpoints.Count);
        }

        [TestMethod]
        public void AddHostEndpoint_EndpointSingletonIsUnaffectedByHandlerLifetime_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", ServiceLifetime.Scoped);

            // The Endpoint metadata must always be a singleton regardless of handler lifetime.
            var endpointDescriptor = services.Single(sd =>
                sd.ServiceType == typeof(Endpoint));

            Assert.AreEqual(ServiceLifetime.Singleton, endpointDescriptor.Lifetime);
        }

        [TestMethod]
        public void AddHostEndpoint_ScopedHandler_TwoResolutionsFromSameScopeAreSame_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", ServiceLifetime.Scoped);

            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            var h1 = scope.ServiceProvider.GetService<FunctionalEndpointHandler>();
            var h2 = scope.ServiceProvider.GetService<FunctionalEndpointHandler>();

            Assert.IsNotNull(h1);
            Assert.AreSame(h1, h2, "Scoped handler should return the same instance within the same scope.");
        }

        [TestMethod]
        public void AddHostEndpoint_SingletonHandler_TwoResolutionsAreAlwaysSame_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func", ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();

            var h1 = provider.GetService<FunctionalEndpointHandler>();
            var h2 = provider.GetService<FunctionalEndpointHandler>();

            Assert.IsNotNull(h1);
            Assert.AreSame(h1, h2, "Singleton handler should always return the same instance.");
        }
    }
}