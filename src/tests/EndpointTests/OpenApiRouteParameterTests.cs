// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 21:06
//
//  Last Modified By  : RzR
//  Last Modified On  : 04-06-2026 23:12
//  ***********************************************************************
//  <copyright file="OpenApiRouteParameterTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S A G E S

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
    public class OpenApiRouteParameterTests
    {
        private static EndpointHostRouter BuildRouter(IEnumerable<Endpoint> endpoints)
            => new(endpoints, MockLogger.Create<EndpointHostRouter>(),
                new EndpointHostOptions());

        [TestMethod]
        public void Build_TemplatedEndpoint_EmitsParametersArray_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("get-user", "/users/{id}", typeof(EndpointOneHandler),
                    new[] { HttpMethod.Get })
            };

            var doc = new OpenApiDocumentBuilder(BuildRouter(endpoints)).Build();

            Assert.IsTrue(doc.Paths.ContainsKey("/users/{id}"));
            var operation = doc.Paths["/users/{id}"]["get"];
            Assert.IsNotNull(operation.Parameters, "Templated endpoint must emit a parameters array");
            Assert.AreEqual(1, operation.Parameters.Count);
        }

        [TestMethod]
        public void Build_TemplatedEndpoint_ParameterHasCorrectFields_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("get-user", "/users/{id}", typeof(EndpointOneHandler),
                    new[] { HttpMethod.Get })
            };

            var doc = new OpenApiDocumentBuilder(BuildRouter(endpoints)).Build();
            var param = doc.Paths["/users/{id}"]["get"].Parameters[0];

            Assert.AreEqual("id", param.Name);
            Assert.AreEqual("path", param.In);
            Assert.IsTrue(param.Required);
            Assert.IsNotNull(param.Schema);
            Assert.AreEqual("string", param.Schema.Type);
        }

        [TestMethod]
        public void Build_MultiParamTemplate_EmitsAllParameters_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("get-item", "/orders/{orderId}/items/{itemId}",
                    typeof(EndpointOneHandler), new[] { HttpMethod.Get })
            };

            var doc = new OpenApiDocumentBuilder(BuildRouter(endpoints)).Build();
            var operation = doc.Paths["/orders/{orderId}/items/{itemId}"]["get"];

            Assert.IsNotNull(operation.Parameters);
            Assert.AreEqual(2, operation.Parameters.Count);
            Assert.AreEqual("orderId", operation.Parameters[0].Name);
            Assert.AreEqual("itemId", operation.Parameters[1].Name);
        }

        [TestMethod]
        public void Build_LiteralEndpoint_NoParametersArray_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("list-users", "/users", typeof(EndpointOneHandler),
                    new[] { HttpMethod.Get })
            };

            var doc = new OpenApiDocumentBuilder(BuildRouter(endpoints)).Build();
            var operation = doc.Paths["/users"]["get"];

            Assert.IsNull(operation.Parameters,
                "Literal endpoint must not emit a parameters array");
        }

        [TestMethod]
        public void BuildJson_TemplatedEndpoint_ParametersInJson_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("get-user", "/users/{id}", typeof(EndpointOneHandler),
                    new[] { HttpMethod.Get })
            };

            var json = new OpenApiDocumentBuilder(BuildRouter(endpoints)).BuildJson();

            using var doc = JsonDocument.Parse(json);
            var parametersEl = doc.RootElement
                .GetProperty("paths")
                .GetProperty("/users/{id}")
                .GetProperty("get")
                .GetProperty("parameters");

            Assert.AreEqual(JsonValueKind.Array, parametersEl.ValueKind);
            Assert.AreEqual(1, parametersEl.GetArrayLength());

            var p = parametersEl[0];
            Assert.AreEqual("id", p.GetProperty("name").GetString());
            Assert.AreEqual("path", p.GetProperty("in").GetString());
            Assert.IsTrue(p.GetProperty("required").GetBoolean());
            Assert.AreEqual("string", p.GetProperty("schema").GetProperty("type").GetString());
        }

        [TestMethod]
        public void BuildJson_LiteralEndpoint_NoParametersProperty_Test()
        {
            var endpoints = new List<Endpoint>
            {
                new("list", "/items", typeof(EndpointOneHandler),
                    new[] { HttpMethod.Get })
            };

            var json = new OpenApiDocumentBuilder(BuildRouter(endpoints)).BuildJson();

            using var doc = JsonDocument.Parse(json);
            var getOp = doc.RootElement
                .GetProperty("paths")
                .GetProperty("/items")
                .GetProperty("get");

            Assert.IsFalse(getOp.TryGetProperty("parameters", out _),
                "Literal endpoint must not serialize a 'parameters' property");
        }

        [TestMethod]
        public void Build_TemplatedEndpointNoMethodConstraint_EmitsGetWithParams_Test()
        {
            var endpoints = new List<Endpoint> { new("get-user", "/users/{id}", typeof(EndpointOneHandler)) };

            var doc = new OpenApiDocumentBuilder(BuildRouter(endpoints)).Build();

            Assert.IsTrue(doc.Paths.ContainsKey("/users/{id}"));
            var pathItem = doc.Paths["/users/{id}"];
            Assert.IsTrue(pathItem.ContainsKey("get"));
            Assert.IsNotNull(pathItem["get"].Parameters);
            Assert.AreEqual(1, pathItem["get"].Parameters.Count);
        }
    }
}
