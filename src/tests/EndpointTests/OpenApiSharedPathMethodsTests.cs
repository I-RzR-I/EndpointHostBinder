// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:11
//  ***********************************************************************
//  <copyright file="OpenApiSharedPathMethodsTests.cs" company="RzR SOFT & TECH">
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Infrastructure.EndpointHosting.Configuration;
using RzR.Infrastructure.EndpointHosting.Host;
using RzR.Infrastructure.EndpointHosting.Models;
using RzR.Infrastructure.EndpointHosting.OpenApi;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class OpenApiSharedPathMethodsTests
    {
        private static EndpointHostRouter BuildRouter(IEnumerable<Endpoint> endpoints)
            => new(endpoints, MockLogger.Create<EndpointHostRouter>(), new EndpointHostOptions());

        [TestMethod]
        public void SamePath_GetAndPost_ProducesSinglePathWithBothOperations_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("list-products", "/products", typeof(EndpointOneHandler), 
                    new[] { HttpMethod.Get }),
                new("create-product", "/products", typeof(EndpointTwoHandler), 
                    new[] { HttpMethod.Post })
            };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            Assert.AreEqual(1, doc.Paths.Count, "Two endpoints on the same path must produce one path entry");
            Assert.IsTrue(doc.Paths.ContainsKey("/products"));

            var pathItem = doc.Paths["/products"];
            Assert.IsTrue(pathItem.ContainsKey("get"), "GET operation must be present");
            Assert.IsTrue(pathItem.ContainsKey("post"), "POST operation must be present");
        }

        [TestMethod]
        public void SamePath_GetAndPost_OperationIdsAreCorrect_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("list-products", "/products", typeof(EndpointOneHandler), new[] { HttpMethod.Get }),
                new("create-product", "/products", typeof(EndpointTwoHandler), new[] { HttpMethod.Post })
            };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            Assert.AreEqual("list-products", doc.Paths["/products"]["get"].OperationId);
            Assert.AreEqual("create-product", doc.Paths["/products"]["post"].OperationId);
        }

        [TestMethod]
        public void SamePath_GetAndPost_JsonContainsBothOperations_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("list-products", "/products", typeof(EndpointOneHandler), new[] { HttpMethod.Get }), 
                new("create-product", "/products", typeof(EndpointTwoHandler), new[] { HttpMethod.Post })
            };

            var router = BuildRouter(endpoints);
            var json = new OpenApiDocumentBuilder(router).BuildJson();

            using var doc = JsonDocument.Parse(json);
            var pathEl = doc.RootElement.GetProperty("paths").GetProperty("/products");

            Assert.IsTrue(pathEl.TryGetProperty("get", out var getOp), "JSON must have a 'get' property");
            Assert.IsTrue(pathEl.TryGetProperty("post", out var postOp), "JSON must have a 'post' property");
            Assert.AreEqual("list-products", getOp.GetProperty("operationId").GetString());
            Assert.AreEqual("create-product", postOp.GetProperty("operationId").GetString());
        }

        [TestMethod]
        public void SamePath_ThreeMethods_AllThreeOperationsPresent_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("get-user", "/users/{id}", typeof(EndpointOneHandler), new[] { HttpMethod.Get }),
                new("update-user", "/users/{id}", typeof(EndpointTwoHandler), new[] { HttpMethod.Put }),
                new("delete-user", "/users/{id}", typeof(EndpointThreeHandler), new[] { HttpMethod.Delete })
            };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            Assert.AreEqual(1, doc.Paths.Count, "Three endpoints on the same path must produce one path entry");

            var pathItem = doc.Paths["/users/{id}"];
            Assert.IsTrue(pathItem.ContainsKey("get"), "GET must be present");
            Assert.IsTrue(pathItem.ContainsKey("put"), "PUT must be present");
            Assert.IsTrue(pathItem.ContainsKey("delete"), "DELETE must be present");
        }
    }
}