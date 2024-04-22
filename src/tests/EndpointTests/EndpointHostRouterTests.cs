// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2024-04-19 21:46
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-19 21:46
// ***********************************************************************
//  <copyright file="EndpointHostRouterTests.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

using EndpointHostBinder.Host;
using EndpointHostBinder.Models;
using EndpointTests.Common;
using EndpointTests.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace EndpointTests
{
    [TestClass]
    public class EndpointHostRouterTests
    {
        private List<Endpoint> _endpoints;
        private EndpointHostRouter _endpointHostRouter;

        [TestInitialize]
        public void Init()
        {
            _endpoints = new List<Endpoint>();
            _endpointHostRouter = new EndpointHostRouter(_endpoints, MockLogger.Create<EndpointHostRouter>());
        }

        [TestMethod]
        public void RouterFind_Null_Test()
        {
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointTwoHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/endpoint0") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFindEndpointOne_Test()
        {
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointTwoHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/endpoint1") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointOneHandler), result.GetType());
        }

        [TestMethod]
        public void RouterFindEndpointTwo_Null_Test()
        {
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointTwoHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/endpoint1") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreNotEqual(typeof(EndpointTwoHandler), result.GetType());
        }

        [TestMethod]
        public void RouterFindEndpointTwo_Test()
        {
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("endpoint2", "/endpoint2", typeof(EndpointTwoHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/endpoint2") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointTwoHandler), result.GetType());
        }

        private class InternalServiceProvider : IServiceProvider
        {
            /// <inheritdoc />
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(EndpointOneHandler))
                    return new EndpointOneHandler();
                if (serviceType == typeof(EndpointTwoHandler))
                    return new EndpointTwoHandler();

                throw new InvalidOperationException();
            }
        }
    }
}