// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 06-06-2026 12:06
// 
//  Last Modified By : RzR
//  Last Modified On : 07-06-2026 01:12
//  ***********************************************************************
//  <copyright file="PathComparisonOptionsTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using EndpointTests.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Discovery;
using System;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class PathComparisonOptionsTests
    {
        [TestMethod]
        public void OrdinalComparison_WrongCase_DoesNotMatch_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder(o => o.PathComparison = StringComparison.Ordinal)
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/Func");

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>();

            var ctx = new DefaultHttpContext { Request = { Path = "/func", Method = "GET" } };

            var result = router.Find(ctx);

            Assert.IsNull(result, "Ordinal comparison: /func must NOT match /Func");
        }

        [TestMethod]
        public void OrdinalComparison_CorrectCase_Matches_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder(o => o.PathComparison = StringComparison.Ordinal)
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/Func");

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>();

            var ctx = new DefaultHttpContext { Request = { Path = "/Func", Method = "GET" } };

            var result = router.Find(ctx);

            Assert.IsNotNull(result, "Ordinal comparison: /Func must match /Func");
        }

        [TestMethod]
        public void DefaultOptions_CaseInsensitive_LowercaseMatchesUppercase_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/Func");

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>();

            var ctx = new DefaultHttpContext { Request = { Path = "/func", Method = "GET" } };

            var result = router.Find(ctx);

            Assert.IsNotNull(result, "Default OrdinalIgnoreCase: /func must match /Func");
        }

        [TestMethod]
        public void DefaultOptions_CaseInsensitive_UppercaseMatchesLowercase_Test()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .RegisterEndpointHostBuilder()
                .AddHostEndpoint<FunctionalEndpointHandler>("func", "/func");

            var provider = services.BuildServiceProvider();
            var router = provider.GetRequiredService<IEndpointHostRouter>();

            var ctx = new DefaultHttpContext { Request = { Path = "/FUNC", Method = "GET" } };

            var result = router.Find(ctx);

            Assert.IsNotNull(result, "Default OrdinalIgnoreCase: /FUNC must match /func");
        }
    }
}