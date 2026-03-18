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
using System.Net.Http;
using System.Threading.Tasks;

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

        [TestMethod]
        public void RouterFind_EmptyEndpointList_ReturnsNull_Test()
        {
            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/anything") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_DisabledEndpoint_ReturnsNull_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), isActive: false));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/ep1") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_ActiveEndpoint_ReturnsHandler_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), isActive: true));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/ep1") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouterFind_UpperCasePath_MatchesCaseInsensitively_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/endpoint1", typeof(EndpointOneHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/ENDPOINT1") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointOneHandler), result.GetType());
        }

        [TestMethod]
        public void RouterFind_MixedCasePath_MatchesCaseInsensitively_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/MyEndpoint", typeof(EndpointOneHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/myENDPOINT") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouterFind_MatchingHttpMethod_ReturnsHandler_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), new[] { HttpMethod.Get }));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/ep1"), Method = "GET" },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointOneHandler), result.GetType());
        }

        [TestMethod]
        public void RouterFind_NonMatchingHttpMethod_ReturnsNull_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), new[] { HttpMethod.Get }));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/ep1"), Method = "POST" },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_HttpMethodCaseInsensitive_ReturnsHandler_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), new[] { HttpMethod.Get }));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/ep1"), Method = "get" },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouterFind_MultipleAllowedMethods_MatchesAllOfThem_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put }));

            foreach (var method in new[] { "GET", "POST", "PUT" })
            {
                var ctx = new DefaultHttpContext
                {
                    Request = { Path = new PathString("/ep1"), Method = method },
                    RequestServices = new InternalServiceProvider()
                };

                var result = _endpointHostRouter.Find(ctx);
                Assert.IsNotNull(result, $"Expected handler for method {method}");
            }
        }

        [TestMethod]
        public void RouterFind_NoAllowedMethodsConstraint_AcceptsAnyMethod_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            foreach (var method in new[] { "GET", "POST", "PUT", "DELETE", "PATCH" })
            {
                var ctx = new DefaultHttpContext
                {
                    Request = { Path = new PathString("/ep1"), Method = method },
                    RequestServices = new InternalServiceProvider()
                };

                var result = _endpointHostRouter.Find(ctx);
                Assert.IsNotNull(result, $"Expected handler for method {method}");
            }
        }

        [TestMethod]
        public void RouterFind_SamePathDifferentMethods_RoutesCorrectly_Test()
        {
            _endpoints.Add(new Endpoint("ep-get", "/resource", typeof(EndpointOneHandler), new[] { HttpMethod.Get }));
            _endpoints.Add(new Endpoint("ep-post", "/resource", typeof(EndpointTwoHandler), new[] { HttpMethod.Post }));

            var getCtx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/resource"), Method = "GET" },
                RequestServices = new InternalServiceProvider()
            };
            var postCtx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/resource"), Method = "POST" },
                RequestServices = new InternalServiceProvider()
            };

            var getResult = _endpointHostRouter.Find(getCtx);
            var postResult = _endpointHostRouter.Find(postCtx);

            Assert.IsNotNull(getResult);
            Assert.AreEqual(typeof(EndpointOneHandler), getResult.GetType());
            Assert.IsNotNull(postResult);
            Assert.AreEqual(typeof(EndpointTwoHandler), postResult.GetType());
        }

        [TestMethod]
        public void RouterExist_MatchingPath_ReturnsTrue_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/ep1") }
            };

            Assert.IsTrue(_endpointHostRouter.Exist(ctx));
        }

        [TestMethod]
        public void RouterExist_NonMatchingPath_ReturnsFalse_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/other") }
            };

            Assert.IsFalse(_endpointHostRouter.Exist(ctx));
        }

        [TestMethod]
        public void RouterExist_EmptyEndpoints_ReturnsFalse_Test()
        {
            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/anything") }
            };

            Assert.IsFalse(_endpointHostRouter.Exist(ctx));
        }

        [TestMethod]
        public async Task RouterExistAsync_MatchingPath_ReturnsTrue_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/ep1") }
            };

            var result = await _endpointHostRouter.ExistAsync(ctx);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task RouterExistAsync_NonMatchingPath_ReturnsFalse_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/nope") }
            };

            var result = await _endpointHostRouter.ExistAsync(ctx);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RouterExist_IsCaseInsensitive_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/endpoint1", typeof(EndpointOneHandler)));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/ENDPOINT1") }
            };

            Assert.IsTrue(_endpointHostRouter.Exist(ctx));
        }

        [TestMethod]
        public void RouterFind_ThreeEndpoints_EachResolvesCorrectly_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("ep2", "/ep2", typeof(EndpointTwoHandler)));
            _endpoints.Add(new Endpoint("ep3", "/ep3", typeof(EndpointThreeHandler)));

            foreach (var (path, expected) in new[]
            {
                ("/ep1", typeof(EndpointOneHandler)),
                ("/ep2", typeof(EndpointTwoHandler)),
                ("/ep3", typeof(EndpointThreeHandler)),
            })
            {
                var ctx = new DefaultHttpContext
                {
                    Request = { Path = new PathString(path) },
                    RequestServices = new InternalServiceProvider()
                };

                var result = _endpointHostRouter.Find(ctx);

                Assert.IsNotNull(result, $"Expected handler for path {path}");
                Assert.AreEqual(expected, result.GetType(), $"Wrong handler type for path {path}");
            }
        }

        [TestMethod]
        public void RouterFind_FirstMatchIsInactive_ReturnsNull_Test()
        {
            // Adds inactive ep1 first; the router's FirstOrDefault picks the inactive one.
            _endpoints.Add(new Endpoint("ep1-inactive", "/resource", typeof(EndpointOneHandler), isActive: false));
            _endpoints.Add(new Endpoint("ep1-active", "/resource", typeof(EndpointTwoHandler), isActive: true));

            var ctx = new DefaultHttpContext
            {
                Request = { Path = new PathString("/resource") },
                RequestServices = new InternalServiceProvider()
            };

            var result = _endpointHostRouter.Find(ctx);

            Assert.IsNull(result);
        }

        private class InternalServiceProvider : IServiceProvider
        {
            /// <inheritdoc />
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(EndpointOneHandler))   return new EndpointOneHandler();
                if (serviceType == typeof(EndpointTwoHandler))   return new EndpointTwoHandler();
                if (serviceType == typeof(EndpointThreeHandler)) return new EndpointThreeHandler();
                if (serviceType == typeof(FunctionalEndpointHandler)) return new FunctionalEndpointHandler();

                throw new InvalidOperationException($"No registration for {serviceType.Name}");
            }
        }
    }
}
