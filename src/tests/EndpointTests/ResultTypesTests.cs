// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 21:06
// 
//  Last Modified By : RzR
//  Last Modified On : 04-06-2026 23:11
//  ***********************************************************************
//  <copyright file="ResultTypesTests.cs" company="RzR SOFT & TECH">
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
    public class ResultTypesTests
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
        public async Task OkResult_ExecuteAsync_Sets200_Test()
        {
            var ctx = CreateContext();
            await EndpointResults.Ok().ExecuteAsync(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
        }

        [TestMethod]
        public void OkResult_Execute_Sets200_Test()
        {
            var ctx = CreateContext();
            EndpointResults.Ok().Execute(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task OkResultOfT_ExecuteAsync_Sets200AndJsonBody_Test()
        {
            var ctx = CreateContext();
            var payload = new { Name = "test", Value = 42 };

            await EndpointResults.Ok(payload).ExecuteAsync(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.Contains("application/json"));

            var body = ReadBody(ctx);
            Assert.IsTrue(body.Contains("test"));
            Assert.IsTrue(body.Contains("42"));
        }

        [TestMethod]
        public void OkResultOfT_Execute_Sets200AndJsonBody_Test()
        {
            var ctx = CreateContext();
            var payload = new { Name = "test", Value = 42 };

            EndpointResults.Ok(payload).Execute(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.Contains("application/json"));

            var body = ReadBody(ctx);
            Assert.IsTrue(body.Contains("test"));
            Assert.IsTrue(body.Contains("42"));
        }

        [TestMethod]
        public async Task JsonResult_ExecuteAsync_Sets200AndApplicationJson_Test()
        {
            var ctx = CreateContext();
            var payload = new { Id = 1, Label = "hello" };

            await EndpointResults.Json(payload).ExecuteAsync(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.StartsWith("application/json"));

            var body = ReadBody(ctx);
            // JsonResult uses System.Text.Json defaults (PascalCase property names for anonymous types)
            Assert.IsTrue(body.Contains("1"), "Body must contain the Id value");
            Assert.IsTrue(body.Contains("hello"), "Body must contain the Label value");
        }

        [TestMethod]
        public async Task JsonResult_ExecuteAsync_CustomStatusCode_Test()
        {
            var ctx = CreateContext();

            await EndpointResults.Json(new { Ok = true }, 201).ExecuteAsync(ctx);

            Assert.AreEqual(201, ctx.Response.StatusCode);
        }

        [TestMethod]
        public void JsonResult_Execute_SerializesPayload_Test()
        {
            var ctx = CreateContext();
            var payload = new { Id = 7 };

            EndpointResults.Json(payload).Execute(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);

            var body = ReadBody(ctx);
            Assert.IsFalse(string.IsNullOrEmpty(body));
        }

        [TestMethod]
        public async Task TextResult_ExecuteAsync_SetsTextPlainAndBody_Test()
        {
            var ctx = CreateContext();

            await EndpointResults.Text("hello world").ExecuteAsync(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.StartsWith("text/plain"));

            var body = ReadBody(ctx);
            Assert.AreEqual("hello world", body);
        }

        [TestMethod]
        public void TextResult_Execute_SetsTextPlainAndBody_Test()
        {
            var ctx = CreateContext();

            EndpointResults.Text("hello world").Execute(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.StartsWith("text/plain"));

            var body = ReadBody(ctx);
            Assert.AreEqual("hello world", body);
        }

        [TestMethod]
        public async Task TextResult_ExecuteAsync_CustomStatusCode_Test()
        {
            var ctx = CreateContext();

            await EndpointResults.Text("created", 201).ExecuteAsync(ctx);

            Assert.AreEqual(201, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task TextResult_ExecuteAsync_CustomContentType_Test()
        {
            var ctx = CreateContext();

            await EndpointResults.Text("<a/>", 200, "text/html; charset=utf-8").ExecuteAsync(ctx);

            Assert.IsTrue(ctx.Response.ContentType.StartsWith("text/html"));
        }

        [TestMethod]
        public async Task NoContentResult_ExecuteAsync_Sets204_Test()
        {
            var ctx = CreateContext();

            await EndpointResults.NoContent().ExecuteAsync(ctx);

            Assert.AreEqual(204, ctx.Response.StatusCode);
        }

        [TestMethod]
        public void NoContentResult_Execute_Sets204_Test()
        {
            var ctx = CreateContext();

            EndpointResults.NoContent().Execute(ctx);

            Assert.AreEqual(204, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task NotFoundResult_ExecuteAsync_Sets404_Test()
        {
            var ctx = CreateContext();

            await EndpointResults.NotFound().ExecuteAsync(ctx);

            Assert.AreEqual(404, ctx.Response.StatusCode);
        }

        [TestMethod]
        public void NotFoundResult_Execute_Sets404_Test()
        {
            var ctx = CreateContext();

            EndpointResults.NotFound().Execute(ctx);

            Assert.AreEqual(404, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task StatusCodeResult_ExecuteAsync_SetsCustomCode_Test()
        {
            var ctx = CreateContext();

            await EndpointResults.StatusCode(418).ExecuteAsync(ctx);

            Assert.AreEqual(418, ctx.Response.StatusCode);
        }

        [TestMethod]
        public void StatusCodeResult_Execute_SetsCustomCode_Test()
        {
            var ctx = CreateContext();

            EndpointResults.StatusCode(503).Execute(ctx);

            Assert.AreEqual(503, ctx.Response.StatusCode);
        }

        [TestMethod]
        public async Task ProblemDetailsResult_ExecuteAsync_Sets500AndProblemJson_Test()
        {
            var ctx = CreateContext();

            await EndpointResults.Problem("Internal error").ExecuteAsync(ctx);

            Assert.AreEqual(500, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.StartsWith("application/problem+json"));

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual("Internal error", doc.RootElement.GetProperty("title").GetString());
            Assert.AreEqual(500, doc.RootElement.GetProperty("status").GetInt32());
        }

        [TestMethod]
        public async Task ProblemDetailsResult_ExecuteAsync_IncludesDetail_Test()
        {
            var ctx = CreateContext();

            await EndpointResults.Problem("Bad", "Something went wrong", 400).ExecuteAsync(ctx);

            Assert.AreEqual(400, ctx.Response.StatusCode);

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual("Bad", doc.RootElement.GetProperty("title").GetString());
            Assert.AreEqual("Something went wrong", doc.RootElement.GetProperty("detail").GetString());
            Assert.AreEqual(400, doc.RootElement.GetProperty("status").GetInt32());
        }

        [TestMethod]
        public void ProblemDetailsResult_Execute_Sets500AndProblemJson_Test()
        {
            var ctx = CreateContext();

            EndpointResults.Problem("Server fault").Execute(ctx);

            Assert.AreEqual(500, ctx.Response.StatusCode);
            Assert.IsTrue(ctx.Response.ContentType.StartsWith("application/problem+json"));

            var body = ReadBody(ctx);
            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual("Server fault", doc.RootElement.GetProperty("title").GetString());
        }

        [TestMethod]
        public async Task ProblemDetailsResult_ExecuteAsync_NullFields_OmittedFromJson_Test()
        {
            var ctx = CreateContext();

            // type, detail, and instance are all null — should be omitted from JSON
            await EndpointResults.Problem("Oops").ExecuteAsync(ctx);

            var body = ReadBody(ctx);
            Assert.IsFalse(body.Contains("\"detail\""));
            Assert.IsFalse(body.Contains("\"type\""));
            Assert.IsFalse(body.Contains("\"instance\""));
        }

        [TestMethod]
        public void OkResultOfT_Execute_NullPayload_WritesJsonNull_Test()
        {
            var ctx = CreateContext();

            EndpointResults.Ok<string>(null).Execute(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.AreEqual("null", ReadBody(ctx));
        }

        [TestMethod]
        public void JsonResult_Execute_NullPayload_WritesJsonNull_Test()
        {
            var ctx = CreateContext();

            EndpointResults.Json<string>(null).Execute(ctx);

            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.AreEqual("null", ReadBody(ctx));
        }
    }
}