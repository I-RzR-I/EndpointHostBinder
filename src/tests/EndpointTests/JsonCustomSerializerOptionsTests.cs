// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:10
//  ***********************************************************************
//  <copyright file="JsonCustomSerializerOptionsTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Infrastructure.EndpointHosting.Results;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class JsonCustomSerializerOptionsTests
    {
        private static DefaultHttpContext CreateContext()
        {
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();

            return ctx;
        }

        private static string ReadBody(DefaultHttpContext ctx)
        {
            ctx.Response.Body.Seek(0, SeekOrigin.Begin);

            return new StreamReader(ctx.Response.Body, Encoding.UTF8).ReadToEnd();
        }

        [TestMethod]
        public async Task JsonResult_CamelCaseOptions_ProducesCamelCasePropertyNames_Test()
        {
            var ctx = CreateContext();
            var payload = new { FirstName = "Bob", LastName = "Smith", Age = 25 };
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            await new JsonResult<object>(payload, 200, options).ExecuteAsync(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.StartsWith("application/json"));

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);

            Assert.IsTrue(doc.RootElement.TryGetProperty("firstName", out var firstNameEl),
                "camelCase: 'firstName' must be in the JSON output");
            Assert.AreEqual("Bob", firstNameEl.GetString());

            Assert.IsTrue(doc.RootElement.TryGetProperty("lastName", out var lastNameEl),
                "camelCase: 'lastName' must be in the JSON output");
            Assert.AreEqual("Smith", lastNameEl.GetString());

            Assert.IsFalse(doc.RootElement.TryGetProperty("FirstName", out _),
                "PascalCase 'FirstName' must NOT appear when CamelCase policy is active");
        }

        [TestMethod]
        public async Task JsonResult_NoOptions_DefaultsArePascalCase_Test()
        {
            // System.Text.Json with no options uses PascalCase for anonymous types.
            var ctx = CreateContext();
            var payload = new { MyProperty = "value" };

            await new JsonResult<object>(payload).ExecuteAsync(ctx);

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);

            Assert.IsTrue(doc.RootElement.TryGetProperty("MyProperty", out _),
                "Without CamelCase policy, property names retain their original casing");
        }

        [TestMethod]
        public async Task JsonResult_CamelCaseOptions_CustomStatusCode_Test()
        {
            var ctx = CreateContext();
            var payload = new { Id = 1 };
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            await new JsonResult<object>(payload, 201, options).ExecuteAsync(ctx);

            Assert.AreEqual(201, ctx.Response.StatusCode);

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);
            Assert.IsTrue(doc.RootElement.TryGetProperty("id", out var idEl));
            Assert.AreEqual(1, idEl.GetInt32());
        }

        [TestMethod]
        public void JsonResult_CamelCaseOptions_Synchronous_ProducesCamelCase_Test()
        {
            var ctx = CreateContext();
            var payload = new { FullName = "Carol", Score = 99 };
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            new JsonResult<object>(payload, 200, options).Execute(ctx);

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);

            Assert.IsTrue(doc.RootElement.TryGetProperty("fullName", out _),
                "Synchronous Execute must also honour CamelCase naming policy");
            Assert.IsFalse(doc.RootElement.TryGetProperty("FullName", out _),
                "PascalCase 'FullName' must not appear when CamelCase policy is active");
        }
    }
}