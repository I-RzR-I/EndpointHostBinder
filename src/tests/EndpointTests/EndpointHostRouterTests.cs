// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 20:06
//
//  Last Modified By  : RzR
//  Last Modified On  : 04-06-2026 23:03
//  ***********************************************************************
//  <copyright file="EndpointHostRouterTests.cs" company="RzR SOFT & TECH">
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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class EndpointHostRouterTests
    {
        private List<Endpoint> _endpoints;

        [TestInitialize]
        public void Init() => _endpoints = new List<Endpoint>();

        // Build the router after endpoints have been added to _endpoints so that
        // the dictionary snapshot includes all registered endpoints.
        private EndpointHostRouter BuildRouter(EndpointHostOptions options = null)
            => options == null
                ? new EndpointHostRouter(_endpoints, MockLogger.Create<EndpointHostRouter>())
                : new EndpointHostRouter(_endpoints, MockLogger.Create<EndpointHostRouter>(), options);

        [TestMethod]
        public void RouterFind_Null_Test()
        {
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointTwoHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/endpoint0")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFindEndpointOne_Test()
        {
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointTwoHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/endpoint1")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointOneHandler), result.EndpointType);
        }

        [TestMethod]
        public void RouterFindEndpointTwo_Null_Test()
        {
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointTwoHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/endpoint1")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreNotEqual(typeof(EndpointTwoHandler), result.EndpointType);
        }

        [TestMethod]
        public void RouterFindEndpointTwo_Test()
        {
            _endpoints.Add(new Endpoint("endpoint1", "/endpoint1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("endpoint2", "/endpoint2", typeof(EndpointTwoHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/endpoint2")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointTwoHandler), result.EndpointType);
        }

        [TestMethod]
        public void RouterFind_EmptyEndpointList_ReturnsNull_Test()
        {
            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/anything")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_DisabledEndpoint_ReturnsNull_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), false));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/ep1")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_ActiveEndpoint_ReturnsHandler_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), true));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/ep1")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouterFind_UpperCasePath_MatchesCaseInsensitively_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/endpoint1", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/ENDPOINT1")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointOneHandler), result.EndpointType);
        }

        [TestMethod]
        public void RouterFind_MixedCasePath_MatchesCaseInsensitively_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/MyEndpoint", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/myENDPOINT")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouterFind_MatchingHttpMethod_ReturnsHandler_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), new[] { HttpMethod.Get }));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/ep1"), Method = "GET"
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointOneHandler), result.EndpointType);
        }

        [TestMethod]
        public void RouterFind_NonMatchingHttpMethod_ReturnsNull_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), new[] { HttpMethod.Get }));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/ep1"), Method = "POST"
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_HttpMethodCaseInsensitive_ReturnsHandler_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler), new[] { HttpMethod.Get }));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/ep1"), Method = "get"
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RouterFind_MultipleAllowedMethods_MatchesAllOfThem_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1",
                typeof(EndpointOneHandler), new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put }));

            var router = BuildRouter();
            foreach (var method in new[] { "GET", "POST", "PUT" })
            {
                var ctx = new DefaultHttpContext
                {
                    Request =
                    {
                        Path = new PathString("/ep1"), Method = method
                    },
                    RequestServices = new InternalServiceProvider()
                };

                var result = router.Find(ctx);
                Assert.IsNotNull(result, $"Expected handler for method {method}");
            }
        }

        [TestMethod]
        public void RouterFind_NoAllowedMethodsConstraint_AcceptsAnyMethod_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            foreach (var method in new[] { "GET", "POST", "PUT", "DELETE", "PATCH" })
            {
                var ctx = new DefaultHttpContext
                {
                    Request =
                    {
                        Path = new PathString("/ep1"), Method = method
                    },
                    RequestServices = new InternalServiceProvider()
                };

                var result = router.Find(ctx);
                Assert.IsNotNull(result, $"Expected handler for method {method}");
            }
        }

        [TestMethod]
        public void RouterFind_SamePathDifferentMethods_RoutesCorrectly_Test()
        {
            _endpoints.Add(new Endpoint("ep-get", "/resource",
                typeof(EndpointOneHandler), new[] { HttpMethod.Get }));
            _endpoints.Add(new Endpoint("ep-post", "/resource",
                typeof(EndpointTwoHandler), new[] { HttpMethod.Post }));

            var router = BuildRouter();

            var getCtx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/resource"), Method = "GET"
                },
                RequestServices = new InternalServiceProvider()
            };
            var postCtx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/resource"), Method = "POST"
                },
                RequestServices = new InternalServiceProvider()
            };

            var getResult = router.Find(getCtx);
            var postResult = router.Find(postCtx);

            Assert.IsNotNull(getResult);
            Assert.AreEqual(typeof(EndpointOneHandler), getResult.EndpointType);
            Assert.IsNotNull(postResult);
            Assert.AreEqual(typeof(EndpointTwoHandler), postResult.EndpointType);
        }

        [TestMethod]
        public void RouterExist_MatchingPath_ReturnsTrue_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/ep1")
                }
            };

            Assert.IsTrue(router.Exist(ctx));
        }

        [TestMethod]
        public void RouterExist_NonMatchingPath_ReturnsFalse_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/other")
                }
            };

            Assert.IsFalse(router.Exist(ctx));
        }

        [TestMethod]
        public void RouterExist_EmptyEndpoints_ReturnsFalse_Test()
        {
            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/anything")
                }
            };

            Assert.IsFalse(router.Exist(ctx));
        }

        [TestMethod]
        public async Task RouterExistAsync_MatchingPath_ReturnsTrue_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/ep1")
                }
            };

            var result = await router.ExistAsync(ctx);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task RouterExistAsync_NonMatchingPath_ReturnsFalse_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/nope")
                }
            };

            var result = await router.ExistAsync(ctx);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RouterExist_IsCaseInsensitive_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/endpoint1", typeof(EndpointOneHandler)));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/ENDPOINT1")
                }
            };

            Assert.IsTrue(router.Exist(ctx));
        }

        [TestMethod]
        public void RouterFind_ThreeEndpoints_EachResolvesCorrectly_Test()
        {
            _endpoints.Add(new Endpoint("ep1", "/ep1", typeof(EndpointOneHandler)));
            _endpoints.Add(new Endpoint("ep2", "/ep2", typeof(EndpointTwoHandler)));
            _endpoints.Add(new Endpoint("ep3", "/ep3", typeof(EndpointThreeHandler)));

            var router = BuildRouter();
            foreach (var (path, expected) in new[]
                     {
                         ("/ep1", typeof(EndpointOneHandler)),
                         ("/ep2", typeof(EndpointTwoHandler)),
                         ("/ep3", typeof(EndpointThreeHandler))
                     })
            {
                var ctx = new DefaultHttpContext
                {
                    Request =
                    {
                        Path = new PathString(path)
                    },
                    RequestServices = new InternalServiceProvider()
                };

                var result = router.Find(ctx);

                Assert.IsNotNull(result, $"Expected handler for path {path}");
                Assert.AreEqual(expected, result.EndpointType, $"Wrong handler type for path {path}");
            }
        }

        [TestMethod]
        public void RouterFind_InactiveBeforeActive_ReturnsActiveEndpoint_Test()
        {
            // Inactive endpoint registered first; Find must skip it and return the active one.
            _endpoints.Add(new Endpoint("ep1-inactive", "/resource", typeof(EndpointOneHandler), false));
            _endpoints.Add(new Endpoint("ep1-active", "/resource", typeof(EndpointTwoHandler), true));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/resource")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointTwoHandler), result.EndpointType);
        }

        [TestMethod]
        public void RouterFind_OnlyInactiveEndpoint_ReturnsNull_Test()
        {
            // A lone inactive endpoint — no active match exists, so Find must return null.
            _endpoints.Add(new Endpoint("ep1-inactive", "/resource", typeof(EndpointOneHandler), false));

            var router = BuildRouter();
            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/resource")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RouterFind_CaseSensitiveOptions_DoesNotMatchDifferentCase_Test()
        {
            // With PathComparison = Ordinal, "/Endpoint1" and "/endpoint1" are different keys.
            _endpoints.Add(new Endpoint("ep1", "/Endpoint1", typeof(EndpointOneHandler)));

            var options = new EndpointHostOptions
            {
                PathComparison = StringComparison.Ordinal
            };
            var router = BuildRouter(options);

            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/endpoint1")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNull(result, "Case-sensitive router must NOT match a path with different casing");
        }

        [TestMethod]
        public void RouterFind_CaseSensitiveOptions_MatchesExactCase_Test()
        {
            // With PathComparison = Ordinal, exact-case lookup must still work.
            _endpoints.Add(new Endpoint("ep1", "/Endpoint1", typeof(EndpointOneHandler)));

            var options = new EndpointHostOptions
            {
                PathComparison = StringComparison.Ordinal
            };
            var router = BuildRouter(options);

            var ctx = new DefaultHttpContext
            {
                Request =
                {
                    Path = new PathString("/Endpoint1")
                },
                RequestServices = new InternalServiceProvider()
            };

            var result = router.Find(ctx);

            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(EndpointOneHandler), result.EndpointType);
        }
    }
}
