// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 21:06
//
//  Last Modified By  : RzR
//  Last Modified On  : 04-06-2026 23:12
//  ***********************************************************************
//  <copyright file="RouterTemplateTests.cs" company="RzR SOFT & TECH">
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
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Infrastructure.EndpointHosting.Configuration;
using RzR.Infrastructure.EndpointHosting.Host;
using RzR.Infrastructure.EndpointHosting.Models;
using RzR.Infrastructure.EndpointHosting.Routing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class RouterTemplateTests
    {
        private List<Endpoint> _endpoints;

        [TestInitialize]
        public void Init() => _endpoints = new List<Endpoint>();

        private EndpointHostRouter BuildRouter(EndpointHostOptions options = null)
            => options == null
                ? new EndpointHostRouter(_endpoints, MockLogger.Create<EndpointHostRouter>())
                : new EndpointHostRouter(_endpoints, MockLogger.Create<EndpointHostRouter>(), options);

        [TestMethod]
        public void RouterFind_SingleParam_Matches_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/42");

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointOneHandler), result.EndpointType);
        }

        [TestMethod]
        public void RouterFind_SingleParam_CapturesValue_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/42");

            router.Find(ctx);

            Assert.AreEqual("42", ctx.GetEndpointRouteValue("id"));
        }

        [TestMethod]
        public void RouterFind_SingleParam_CapturesValue_ViaGetRouteValues_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/hello-world");

            router.Find(ctx);

            var values = ctx.GetEndpointRouteValues();
            Assert.IsNotNull(values);
            Assert.IsTrue(values.ContainsKey("id"));
            Assert.AreEqual("hello-world", values["id"]);
        }

        [TestMethod]
        public void RouterFind_MultipleParams_Matches_Test()
        {
            _endpoints.Add(new Endpoint("get-item", "/orders/{orderId}/items/{itemId}",
                typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/orders/99/items/7");

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouterFind_MultipleParams_CapturesBothValues_Test()
        {
            _endpoints.Add(new Endpoint("get-item", "/orders/{orderId}/items/{itemId}",
                typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/orders/99/items/7");

            router.Find(ctx);

            Assert.AreEqual("99", ctx.GetEndpointRouteValue("orderId"));
            Assert.AreEqual("7", ctx.GetEndpointRouteValue("itemId"));
        }

        [TestMethod]
        public void RouterFind_LiteralWinsOverTemplate_Test()
        {
            // Template registered first; literal registered second — literal must still win.
            _endpoints.Add(new Endpoint("template-ep", "/users/{id}", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("literal-ep", "/users/me", typeof(EndpointTwoHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/me");

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointTwoHandler), result.EndpointType,
                "Literal /users/me must win over template /users/{id}");
        }

        [TestMethod]
        public void RouterFind_LiteralWinsOverTemplate_NoRouteValuesWritten_Test()
        {
            _endpoints.Add(new Endpoint("template-ep", "/users/{id}", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("literal-ep", "/users/me", typeof(EndpointTwoHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/me");

            router.Find(ctx);

            // Literal match — no route values should be written.
            var values = ctx.GetEndpointRouteValues();
            Assert.AreEqual(0, values.Count,
                "Literal match must not write any route-value entries");
        }

        [TestMethod]
        public void RouterFind_MostSpecificTemplate_Wins_Test()
        {
            // Two templates both match /a/users/42:
            //   /a/{section}/{id} — 1 literal ("a"), 2 params, LiteralCount = 1
            //   /a/users/{id} — 2 literals ("a","users"), 1 param, LiteralCount = 2 wins
            _endpoints.Add(new Endpoint("generic", "/a/{section}/{id}", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("specific", "/a/users/{id}", typeof(EndpointTwoHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/a/users/42");

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointTwoHandler), result.EndpointType,
                "/a/users/{id} has more literal segments and must be preferred");
        }

        [TestMethod]
        public void RouterFind_SegmentCountMismatch_ReturnsNull_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            // /users/42/extra has 3 segments, template has 2
            var ctx = MakeContext("/users/42/extra");

            var result = router.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_TooFewSegments_ReturnsNull_Test()
        {
            _endpoints.Add(new Endpoint("get-item", "/orders/{orderId}/items/{itemId}",
                typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/orders/99");

            var result = router.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_Template_CaseInsensitiveByDefault_MatchesLiteralSegment_Test()
        {
            _endpoints.Add(new Endpoint("ep", "/Users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter(); // default: OrdinalIgnoreCase
            var ctx = MakeContext("/users/99");

            var result = router.Find(ctx);

            Assert.IsNotNull(result, "Default options are case-insensitive — /users/99 must match /Users/{id}");
        }

        [TestMethod]
        public void RouterFind_Template_CaseSensitive_DoesNotMatchDifferentCase_Test()
        {
            _endpoints.Add(new Endpoint("ep", "/Users/{id}", typeof(EndpointOneHandler)));

            var options = new EndpointHostOptions { PathComparison = StringComparison.Ordinal };
            var router = BuildRouter(options);
            var ctx = MakeContext("/users/99"); // lowercase — different case

            var result = router.Find(ctx);

            Assert.IsNull(result, "Ordinal options must NOT match /users/99 against /Users/{id}");
        }

        [TestMethod]
        public void RouterFind_Template_CaseSensitive_MatchesExactCase_Test()
        {
            _endpoints.Add(new Endpoint("ep", "/Users/{id}", typeof(EndpointOneHandler)));

            var options = new EndpointHostOptions { PathComparison = StringComparison.Ordinal };
            var router = BuildRouter(options);
            var ctx = MakeContext("/Users/99"); // exact case

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual("99", ctx.GetEndpointRouteValue("id"));
        }

        [TestMethod]
        public void RouterFind_Template_MatchingMethod_ReturnsEndpoint_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}",
                typeof(EndpointOneHandler), new[] { HttpMethod.Get }));

            var router = BuildRouter();
            var ctx = MakeContext("/users/5", "GET");

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouterFind_Template_NonMatchingMethod_ReturnsNull_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}",
                typeof(EndpointOneHandler), new[] { HttpMethod.Get }));

            var router = BuildRouter();
            var ctx = MakeContext("/users/5", "DELETE");

            var result = router.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_Template_MethodCaseInsensitive_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}",
                typeof(EndpointOneHandler), new[] { HttpMethod.Get }));

            var router = BuildRouter();
            var ctx = MakeContext("/users/5", "get");

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouterFind_InactiveTemplateDoesNotShadowActive_Test()
        {
            // Inactive template registered first; it must be skipped.
            _endpoints.Add(new Endpoint("inactive", "/users/{id}", typeof(EndpointOneHandler),
                false));
            _endpoints.Add(new Endpoint("active", "/users/{id}", typeof(EndpointTwoHandler),
                true));

            var router = BuildRouter();
            var ctx = MakeContext("/users/7");

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointTwoHandler), result.EndpointType,
                "Active template endpoint must win over inactive one with the same template");
        }

        [TestMethod]
        public void RouterFind_OnlyInactiveTemplate_ReturnsNull_Test()
        {
            _endpoints.Add(new Endpoint("inactive", "/users/{id}", typeof(EndpointOneHandler),
                false));

            var router = BuildRouter();
            var ctx = MakeContext("/users/7");

            var result = router.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_LiteralPath_NoRouteValuesWritten_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/items", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/items");

            router.Find(ctx);

            var values = ctx.GetEndpointRouteValues();
            Assert.AreEqual(0, values.Count,
                "Literal endpoint match must produce an empty route-values dictionary");
        }

        [TestMethod]
        public void GetEndpointRouteValue_UnknownName_ReturnsNull_Test()
        {
            _endpoints.Add(new Endpoint("ep", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/99");
            router.Find(ctx);

            var val = ctx.GetEndpointRouteValue("nonexistent");

            Assert.IsNull(val);
        }

        [TestMethod]
        public void RouterFind_SingleParam_GuidValue_Matches_Test()
        {
            const string guidValue = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/" + guidValue);

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointOneHandler), result.EndpointType);
        }

        [TestMethod]
        public void RouterFind_SingleParam_GuidValue_CapturedAsString_Test()
        {
            const string guidValue = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/" + guidValue);

            router.Find(ctx);

            var captured = ctx.GetEndpointRouteValue("id");
            Assert.AreEqual(guidValue, captured);
        }

        [TestMethod]
        public void RouterFind_SingleParam_GuidValue_ParsedSuccessfully_Test()
        {
            const string guidValue = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/" + guidValue);

            router.Find(ctx);

            var captured = ctx.GetEndpointRouteValue("id");
            var parsed = Guid.Parse(captured);
            Assert.AreEqual(guidValue, parsed.ToString());
        }

        [TestMethod]
        public void RouterFind_MultipleParams_GuidAndInt_BothCaptured_Test()
        {
            const string orderId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
            const string itemId = "42";
            _endpoints.Add(new Endpoint("get-item", "/orders/{orderId}/items/{itemId}",
                typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/orders/" + orderId + "/items/" + itemId);

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(orderId, ctx.GetEndpointRouteValue("orderId"));
            Assert.AreEqual(itemId, ctx.GetEndpointRouteValue("itemId"));
        }

        [TestMethod]
        public void RouterFind_MultipleParams_GuidAndInt_BothParseable_Test()
        {
            const string orderId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
            const string itemId = "42";
            _endpoints.Add(new Endpoint("get-item", "/orders/{orderId}/items/{itemId}",
                typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/orders/" + orderId + "/items/" + itemId);

            router.Find(ctx);

            Assert.AreEqual(new Guid(orderId), Guid.Parse(ctx.GetEndpointRouteValue("orderId")));
            Assert.AreEqual(42, int.Parse(ctx.GetEndpointRouteValue("itemId")));
        }

        [TestMethod]
        public void RouterFind_SameTemplate_MatchesBothIntAndGuid_Test()
        {
            const string guidValue = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
            const string intValue = "99";
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();

            var ctxInt = MakeContext("/users/" + intValue);
            var resultInt = router.Find(ctxInt);

            var ctxGuid = MakeContext("/users/" + guidValue);
            var resultGuid = router.Find(ctxGuid);

            Assert.IsNotNull(resultInt, "Template must match integer path segment");
            Assert.AreEqual(intValue, ctxInt.GetEndpointRouteValue("id"));

            Assert.IsNotNull(resultGuid, "Template must match GUID path segment");
            Assert.AreEqual(guidValue, ctxGuid.GetEndpointRouteValue("id"));
        }

        [TestMethod]
        public void Exist_TemplateMatch_DoesNotWriteRouteValues_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/42");

            var exists = router.Exist(ctx);

            Assert.IsTrue(exists, "Exist must return true for a matching path");
            var values = ctx.GetEndpointRouteValues();
            Assert.AreEqual(0, values.Count,
                "Exist must not write route values into HttpContext.Items");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task ExistAsync_TemplateMatch_DoesNotWriteRouteValues_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/42");

            var exists = await router.ExistAsync(ctx);

            Assert.IsTrue(exists, "ExistAsync must return true for a matching path");
            var values = ctx.GetEndpointRouteValues();
            Assert.AreEqual(0, values.Count,
                "ExistAsync must not write route values into HttpContext.Items");
        }

        [TestMethod]
        public void Find_TemplateMatch_StillWritesRouteValues_Regression_Test()
        {
            _endpoints.Add(new Endpoint("get-user", "/users/{id}", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/users/99");

            var result = router.Find(ctx);

            Assert.IsNotNull(result, "Find must still return a match");
            Assert.AreEqual("99", ctx.GetEndpointRouteValue("id"),
                "Find must still write route values into HttpContext.Items");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RouterBuild_EmptyParamName_ThrowsArgumentException_Test()
        {
            // Path "/{}" has an empty parameter name — must throw during router construction.
            _endpoints.Add(new Endpoint("bad-ep", "/{}", typeof(EndpointOneHandler)));
            BuildRouter();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RouterBuild_DuplicateParamName_ThrowsArgumentException_Test()
        {
            // Path "/{id}/{id}" duplicates parameter name "id" — must throw.
            _endpoints.Add(new Endpoint("dup-ep", "/{id}/{id}", typeof(EndpointOneHandler)));
            BuildRouter();
        }

        [TestMethod]
        public void RouterFind_ValidMultiParam_CapturesBothValues_Test()
        {
            _endpoints.Add(new Endpoint("multi-ep", "/shop/{category}/{product}",
                typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = MakeContext("/shop/books/978");

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual("books", ctx.GetEndpointRouteValue("category"));
            Assert.AreEqual("978", ctx.GetEndpointRouteValue("product"));
        }

        private static DefaultHttpContext MakeContext(string path, string method = "GET")
            => new() { Request = { Path = new PathString(path), Method = method } };
    }
}
