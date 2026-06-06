// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 21:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:06
//  ***********************************************************************
//  <copyright file="GetEndpointsTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using EndpointTests.Helpers;
using EndpointTests.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Infrastructure.EndpointHosting.Configuration;
using RzR.Infrastructure.EndpointHosting.Host;
using RzR.Infrastructure.EndpointHosting.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class GetEndpointsTests
    {
        private List<Endpoint> _endpoints;

        [TestInitialize]
        public void Init() => _endpoints = new List<Endpoint>();

        private EndpointHostRouter BuildRouter() => new(_endpoints, MockLogger.Create<EndpointHostRouter>(), new EndpointHostOptions());

        [TestMethod]
        public void GetEndpoints_EmptyRouter_ReturnsEmptyCollection_Test()
        {
            var router = BuildRouter();

            var result = router.GetEndpoints();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetEndpoints_SingleEndpoint_ReturnsOneItem_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));
            var router = BuildRouter();

            var result = router.GetEndpoints();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetEndpoints_ThreeEndpoints_ReturnsThreeItems_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("ep2", "/ep2", typeof(EndpointTwoHandler)));
            _endpoints.Add(new Endpoint("ep3", "/ep3", typeof(EndpointThreeHandler)));
            var router = BuildRouter();

            var result = router.GetEndpoints();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void GetEndpoints_NamesMatchRegistrationOrder_Test()
        {
            _endpoints.Add(new Endpoint("alpha", "/alpha", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("beta", "/beta", typeof(EndpointTwoHandler)));
            var router = BuildRouter();

            var result = router.GetEndpoints().ToList();

            Assert.AreEqual("alpha", result[0].Name);
            Assert.AreEqual("beta", result[1].Name);
        }

        [TestMethod]
        public void GetEndpoints_PathsMatchRegistration_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/path/one", typeof(EndpointOneHandler)));
            var router = BuildRouter();

            var ep = router.GetEndpoints().Single();

            Assert.AreEqual("/path/one", ep.Path.Value);
        }

        [TestMethod]
        public void GetEndpoints_InactiveEndpoint_StillReturned_Test()
        {
            _endpoints.Add(new Endpoint("inactive", "/inactive", typeof(EndpointOneHandler), false));
            var router = BuildRouter();

            var result = router.GetEndpoints();

            Assert.AreEqual(1, result.Count);
            Assert.IsFalse(result.Single().IsActive);
        }

        [TestMethod]
        public void GetEndpoints_IncludesEndpointsWithMethodConstraints_Test()
        {
            _endpoints.Add(new Endpoint("get-ep", "/items", typeof(EndpointOneHandler),
                new[] { HttpMethod.Get, HttpMethod.Post }));
            var router = BuildRouter();

            var ep = router.GetEndpoints().Single();

            Assert.IsNotNull(ep.AllowedMethods);
            Assert.AreEqual(2, ep.AllowedMethods.Count());
        }

        [TestMethod]
        public void GetEndpoints_ReturnedCollectionIsReadOnly_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));
            var router = BuildRouter();

            var result = router.GetEndpoints();

            // IReadOnlyCollection<T> — must not be castable to a mutable list
            Assert.IsFalse(result is List<Endpoint>,
                "GetEndpoints must return a read-only view, not the mutable backing list");
        }

        [TestMethod]
        public void GetEndpoints_CalledTwice_ReturnsSameCount_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));
            var router = BuildRouter();

            var first = router.GetEndpoints();
            var second = router.GetEndpoints();

            Assert.AreEqual(first.Count, second.Count);
        }
    }
}