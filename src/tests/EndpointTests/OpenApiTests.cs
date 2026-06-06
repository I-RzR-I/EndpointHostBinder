// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 21:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:10
//  ***********************************************************************
//  <copyright file="OpenApiTests.cs" company="RzR SOFT & TECH">
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
using RzR.Infrastructure.EndpointHosting.OpenApi;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class OpenApiTests
    {
        private static EndpointHostRouter BuildRouter(IEnumerable<Endpoint> endpoints) 
            => new(endpoints, MockLogger.Create<EndpointHostRouter>(), new EndpointHostOptions());

        [TestMethod]
        public void Build_EmptyRouter_ProducesValidDocumentStructure_Test()
        {
            var router = BuildRouter(new List<Endpoint>());
            var builder = new OpenApiDocumentBuilder(router);

            var doc = builder.Build();

            Assert.AreEqual("3.0.1", doc.Openapi);
            Assert.IsNotNull(doc.Info);
            Assert.AreEqual(OpenApiDocumentBuilder.DefaultTitle, doc.Info.Title);
            Assert.AreEqual(OpenApiDocumentBuilder.DefaultVersion, doc.Info.Version);
            Assert.IsNotNull(doc.Paths);
        }

        [TestMethod]
        public void Build_CustomTitleAndVersion_AppearsInDocument_Test()
        {
            var router = BuildRouter(new List<Endpoint>());
            var builder = new OpenApiDocumentBuilder(router, "My API", "2.3.4");

            var doc = builder.Build();

            Assert.AreEqual("My API", doc.Info.Title);
            Assert.AreEqual("2.3.4", doc.Info.Version);
        }

        [TestMethod]
        public void Build_EndpointWithNoMethods_EmitsGetOperation_Test()
        {
            var endpoints = new List<Endpoint> { new("my-endpoint", "/api/resource", typeof(EndpointOneHandler)) };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            Assert.IsTrue(doc.Paths.ContainsKey("/api/resource"), "Path /api/resource must be present");
            var pathItem = doc.Paths["/api/resource"];
            Assert.IsTrue(pathItem.ContainsKey("get"), "Endpoint with no method constraint must produce a 'get' operation");
        }

        [TestMethod]
        public void Build_EndpointWithGetMethod_EmitsGetOperation_Test()
        {
            var endpoints = new List<Endpoint> { new("get-ep", "/api/items", 
                typeof(EndpointOneHandler), new[] { HttpMethod.Get }) };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            Assert.IsTrue(doc.Paths.ContainsKey("/api/items"));
            Assert.IsTrue(doc.Paths["/api/items"].ContainsKey("get"));
        }

        [TestMethod]
        public void Build_EndpointWithPostMethod_EmitsPostOperation_Test()
        {
            var endpoints = new List<Endpoint> { new("post-ep", "/api/items",
                typeof(EndpointOneHandler), new[] { HttpMethod.Post }) };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            Assert.IsTrue(doc.Paths.ContainsKey("/api/items"));
            Assert.IsTrue(doc.Paths["/api/items"].ContainsKey("post"));
        }

        [TestMethod]
        public void Build_EndpointWithMultipleMethods_EmitsAllOperations_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("multi-ep", "/api/multi", typeof(EndpointOneHandler),
                    new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Delete })
            };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            var pathItem = doc.Paths["/api/multi"];
            Assert.IsTrue(pathItem.ContainsKey("get"), "GET operation must be present");
            Assert.IsTrue(pathItem.ContainsKey("post"), "POST operation must be present");
            Assert.IsTrue(pathItem.ContainsKey("delete"), "DELETE operation must be present");
        }

        [TestMethod]
        public void Build_TwoEndpointsDistinctPaths_BothPathsPresent_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("ep-one", "/api/one", typeof(EndpointOneHandler), 
                    new[] { HttpMethod.Get }), new("ep-two", "/api/two", 
                    typeof(EndpointTwoHandler), new[] { HttpMethod.Post })
            };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            Assert.IsTrue(doc.Paths.ContainsKey("/api/one"), "/api/one must be present");
            Assert.IsTrue(doc.Paths.ContainsKey("/api/two"), "/api/two must be present");
        }

        [TestMethod]
        public void Build_OperationId_DerivedFromEndpointName_Test()
        {
            var endpoints = new List<Endpoint> { new("GetAllItems", "/api/items", 
                typeof(EndpointOneHandler), new[] { HttpMethod.Get }) };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            var operation = doc.Paths["/api/items"]["get"];
            Assert.AreEqual("GetAllItems", operation.OperationId);
        }

        [TestMethod]
        public void Build_NoMethodConstraint_OperationIdMatchesName_Test()
        {
            var endpoints = new List<Endpoint> { new("MyHandler", "/api/handler", typeof(EndpointOneHandler)) };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            var operation = doc.Paths["/api/handler"]["get"];
            Assert.AreEqual("MyHandler", operation.OperationId);
        }

        [TestMethod]
        public void Build_AllOperations_Have200SuccessResponse_Test()
        {
            var endpoints = new List<Endpoint> { new("ep1", "/a", typeof(EndpointOneHandler),
                new[] { HttpMethod.Get }), new("ep2", "/b", typeof(EndpointTwoHandler),
                new[] { HttpMethod.Post }) };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            foreach (var path in doc.Paths.Values)
            {
                foreach (var operation in path.Values)
                {
                    Assert.IsTrue(operation.Responses.ContainsKey("200"),
                        $"Operation '{operation.OperationId}' must have a 200 response");
                    Assert.AreEqual("Success", operation.Responses["200"].Description);
                }
            }
        }

        [TestMethod]
        public void BuildJson_ProducesValidJson_Test()
        {
            var endpoints = new List<Endpoint> { new("ep1", "/api/test", typeof(EndpointOneHandler),
                new[] { HttpMethod.Get }) };

            var router = BuildRouter(endpoints);
            var json = new OpenApiDocumentBuilder(router).BuildJson();

            Assert.IsFalse(string.IsNullOrEmpty(json));

            // Must be parseable JSON.
            using var doc = JsonDocument.Parse(json);
            Assert.AreEqual("3.0.1", doc.RootElement.GetProperty("openapi").GetString());
        }

        [TestMethod]
        public void BuildJson_TitleVersionAndPaths_PresentInJson_Test()
        {
            var endpoints = new List<Endpoint> { new("list-users", "/users", typeof(EndpointOneHandler), 
                new[] { HttpMethod.Get }) };

            var router = BuildRouter(endpoints);
            var json = new OpenApiDocumentBuilder(router, "Users API", "3.0.0").BuildJson();

            using var doc = JsonDocument.Parse(json);
            Assert.AreEqual("3.0.1", doc.RootElement.GetProperty("openapi").GetString());
            Assert.AreEqual("Users API", doc.RootElement.GetProperty("info").GetProperty("title").GetString());
            Assert.AreEqual("3.0.0", doc.RootElement.GetProperty("info").GetProperty("version").GetString());

            var paths = doc.RootElement.GetProperty("paths");
            Assert.IsTrue(paths.TryGetProperty("/users", out _), "/users path must appear in JSON");
        }

        [TestMethod]
        public void BuildJson_OperationId_InJson_Test()
        {
            var endpoints = new List<Endpoint> { new("ListOrders", "/orders", typeof(EndpointOneHandler),
                new[] { HttpMethod.Get }) };

            var router = BuildRouter(endpoints);
            var json = new OpenApiDocumentBuilder(router).BuildJson();

            using var doc = JsonDocument.Parse(json);
            var operation = doc.RootElement
                .GetProperty("paths")
                .GetProperty("/orders")
                .GetProperty("get");

            Assert.AreEqual("ListOrders", operation.GetProperty("operationId").GetString());
        }

        [TestMethod]
        public void BuildJson_NullOrEmptyTitleVersion_UsesDefaults_Test()
        {
            var router = BuildRouter(new List<Endpoint>());
            var builder = new OpenApiDocumentBuilder(router, null, null);
            var doc = builder.Build();

            Assert.AreEqual(OpenApiDocumentBuilder.DefaultTitle, doc.Info.Title);
            Assert.AreEqual(OpenApiDocumentBuilder.DefaultVersion, doc.Info.Version);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_NullRouter_ThrowsArgumentNullException_Test() => _ = new OpenApiDocumentBuilder(null);

        [TestMethod]
        public void Build_InactiveEndpoint_ExcludedByDefault_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("active-ep", "/api/active", typeof(EndpointOneHandler), true),
                new("inactive-ep", "/api/inactive", typeof(EndpointTwoHandler), false)
            };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build();

            Assert.IsTrue(doc.Paths.ContainsKey("/api/active"), "Active endpoint must appear in document");
            Assert.IsFalse(doc.Paths.ContainsKey("/api/inactive"),
                "Inactive endpoint must be excluded by default");
        }

        [TestMethod]
        public void Build_InactiveEndpoint_IncludedWhenOptIn_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("active-ep", "/api/active", typeof(EndpointOneHandler), true),
                new("inactive-ep", "/api/inactive", typeof(EndpointTwoHandler), false)
            };

            var router = BuildRouter(endpoints);
            var doc = new OpenApiDocumentBuilder(router).Build(includeInactive: true);

            Assert.IsTrue(doc.Paths.ContainsKey("/api/active"), "Active endpoint must appear");
            Assert.IsTrue(doc.Paths.ContainsKey("/api/inactive"),
                "Inactive endpoint must appear when includeInactive:true");
        }

        [TestMethod]
        public void BuildJson_InactiveEndpoint_ExcludedByDefault_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("active-ep", "/api/active", typeof(EndpointOneHandler), true),
                new("inactive-ep", "/api/inactive", typeof(EndpointTwoHandler), false)
            };

            var router = BuildRouter(endpoints);
            var json = new OpenApiDocumentBuilder(router).BuildJson();

            using var doc = JsonDocument.Parse(json);
            var paths = doc.RootElement.GetProperty("paths");
            Assert.IsTrue(paths.TryGetProperty("/api/active", out _), "Active path must be in JSON");
            Assert.IsFalse(paths.TryGetProperty("/api/inactive", out _),
                "Inactive path must not appear in JSON by default");
        }

        [TestMethod]
        public void BuildJson_InactiveEndpoint_IncludedWhenOptIn_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("active-ep", "/api/active", typeof(EndpointOneHandler), true),
                new("inactive-ep", "/api/inactive", typeof(EndpointTwoHandler), false)
            };

            var router = BuildRouter(endpoints);
            var json = new OpenApiDocumentBuilder(router).BuildJson(includeInactive: true);

            using var doc = JsonDocument.Parse(json);
            var paths = doc.RootElement.GetProperty("paths");
            Assert.IsTrue(paths.TryGetProperty("/api/inactive", out _),
                "Inactive path must appear in JSON when includeInactive:true");
        }
    }
}