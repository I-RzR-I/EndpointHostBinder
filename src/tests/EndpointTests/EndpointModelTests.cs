// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointTests
//  Author           : RzR
//  Created On       : 2026-03-18 20:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-18 20:32
// ***********************************************************************
//  <copyright file="EndpointModelTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using EndpointHostBinder.Models;
using EndpointTests.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class EndpointModelTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Endpoint_NullName_ThrowsArgumentException() => _ = new Endpoint(null, "/path", typeof(EndpointOneHandler));

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Endpoint_EmptyName_ThrowsArgumentException() => _ = new Endpoint("", "/path", typeof(EndpointOneHandler));

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Endpoint_WhitespaceName_ThrowsArgumentException() => _ = new Endpoint("   ", "/path", typeof(EndpointOneHandler));

        // ---------------------------------------------------------------
        // Type validation
        // ---------------------------------------------------------------

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Endpoint_NullType_ThrowsArgumentNullException() => _ = new Endpoint("ep", "/path", null);

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Endpoint_TypeNotImplementingInterface_ThrowsArgumentException() =>
            // string does not implement IEndpointHostRequestHandler
            _ = new Endpoint("ep", "/path", typeof(string));

        [TestMethod]
        public void Endpoint_ValidArgs_PropertiesSetCorrectly()
        {
            var ep = new Endpoint("myEndpoint", "/mypath", typeof(EndpointOneHandler));

            Assert.AreEqual("myEndpoint", ep.Name);
            Assert.AreEqual(new PathString("/mypath"), ep.Path);
            Assert.AreEqual(typeof(EndpointOneHandler), ep.EndpointType);
            Assert.IsTrue(ep.IsActive);
            Assert.IsNull(ep.AllowedMethods);
        }

        [TestMethod]
        public void Endpoint_IsActiveFalse_PropertyCorrect()
        {
            var ep = new Endpoint("ep", "/path", typeof(EndpointOneHandler), false);

            Assert.IsFalse(ep.IsActive);
        }

        [TestMethod]
        public void Endpoint_IsActiveTrue_PropertyCorrect()
        {
            var ep = new Endpoint("ep", "/path", typeof(EndpointOneHandler), true);

            Assert.IsTrue(ep.IsActive);
        }

        [TestMethod]
        public void Endpoint_AllowedMethodsSetCorrectly()
        {
            var ep = new Endpoint("ep", "/path", typeof(EndpointOneHandler), new[] { "GET", "POST" });

            Assert.IsNotNull(ep.AllowedMethods);
            CollectionAssert.AreEquivalent(new[] { "GET", "POST" }, ep.AllowedMethods.ToArray());
        }

        [TestMethod]
        public void Endpoint_NullAllowedMethods_PropertyIsNull()
        {
            var ep = new Endpoint("ep", "/path", typeof(EndpointOneHandler), null);

            Assert.IsNull(ep.AllowedMethods);
        }

        [TestMethod]
        public void Endpoint_EmptyAllowedMethods_PropertyIsNull()
        {
            var ep = new Endpoint("ep", "/path", typeof(EndpointOneHandler), new string[0]);

            Assert.IsNull(ep.AllowedMethods);
        }

        [TestMethod]
        public void Endpoint_AllowedMethodsWithIsActive_AllPropertiesCorrect()
        {
            var ep = new Endpoint("ep", "/path", typeof(EndpointOneHandler), false, new[] { "DELETE" });

            Assert.IsFalse(ep.IsActive);
            Assert.IsNotNull(ep.AllowedMethods);
            CollectionAssert.AreEquivalent(new[] { "DELETE" }, ep.AllowedMethods.ToArray());
        }

        [TestMethod]
        public void Endpoint_DerivedHandlerType_AcceptedByIsAssignableFrom()
        {
            // EndpointOneHandler → EndpointOneHandlerDerived should still be valid
            var ep = new Endpoint("ep", "/path", typeof(EndpointOneHandlerDerived));

            Assert.AreEqual(typeof(EndpointOneHandlerDerived), ep.EndpointType);
        }

        private class EndpointOneHandlerDerived : EndpointOneHandler
        {
        }
    }
}