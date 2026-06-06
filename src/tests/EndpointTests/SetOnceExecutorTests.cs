// ***********************************************************************
//  Assembly          : RzR.Shared.Services.EndpointTests
//  Author            : RzR
//  Created           : 04-06-2026 21:06
//
//  Last Modified By  : RzR
//  Last Modified On  : 04-06-2026 23:14
//  ***********************************************************************
//  <copyright file="SetOnceExecutorTests.cs" company="RzR SOFT & TECH">
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
using RzR.Infrastructure.EndpointHosting.Models;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointTests
{
    [TestClass]
    public class SetOnceExecutorTests
    {
        [TestMethod]
        public void SetExecutor_OnEndpointWithNullExecutor_Succeeds_Test()
        {
            var endpoint = new Endpoint("ep", "/ep", typeof(EndpointOneHandler));

            Assert.IsNull(endpoint.Executor, "Executor should be null after 4-arg constructor.");

            var executor = new StubExecutor();
            endpoint.SetExecutor(executor);

            Assert.IsNotNull(endpoint.Executor);
            Assert.AreSame(executor, endpoint.Executor);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetExecutor_WhenExecutorAlreadySetViaConstructor_ThrowsInvalidOperationException_Test()
        {
            var executor = new StubExecutor();
            var endpoint = new Endpoint("ep", "/ep", typeof(EndpointOneHandler), true,
                Array.Empty<HttpMethod>(), executor);

            // Executor is already set via the 6-arg constructor — second call must throw.
            endpoint.SetExecutor(new StubExecutor());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetExecutor_CalledTwice_SecondCallThrows_Test()
        {
            var endpoint = new Endpoint("ep", "/ep", typeof(EndpointOneHandler));
            var executor = new StubExecutor();

            endpoint.SetExecutor(executor);
            // Second call on the same instance must throw.
            endpoint.SetExecutor(new StubExecutor());
        }

        [TestMethod]
        public void Endpoint_CtorWithExecutor_ExecutorPropertyIsSet_Test()
        {
            var executor = new StubExecutor();

            var endpoint = new Endpoint("ep", "/ep", typeof(EndpointOneHandler), true,
                Array.Empty<HttpMethod>(), executor);

            Assert.IsNotNull(endpoint.Executor);
            Assert.AreSame(executor, endpoint.Executor);
        }

        [TestMethod]
        public void Endpoint_CtorWithExecutor_AllPropertiesCorrect_Test()
        {
            var executor = new StubExecutor();
            var endpoint = new Endpoint("exec-ep", "/exec-ep", typeof(EndpointOneHandler), true,
                new[] { HttpMethod.Get }, executor);

            Assert.AreEqual("exec-ep", endpoint.Name);
            Assert.IsTrue(endpoint.IsActive);
            Assert.IsNotNull(endpoint.AllowedMethods);
            Assert.IsNotNull(endpoint.Executor);
        }

        [TestMethod]
        public void SetExecutor_ErrorMessageContainsEndpointName_Test()
        {
            const string name = "my-named-endpoint";
            var executor = new StubExecutor();
            var endpoint = new Endpoint(name, "/ep", typeof(EndpointOneHandler), true,
                Array.Empty<HttpMethod>(), executor);

            try
            {
                endpoint.SetExecutor(new StubExecutor());
                Assert.Fail("Expected InvalidOperationException was not thrown.");
            }
            catch (InvalidOperationException ex)
            {
                StringAssert.Contains(ex.Message, name,
                    "Exception message should include the endpoint name to aid diagnosis.");
            }
        }

        [TestMethod]
        public void SetExecutor_Concurrent_ExactlyOneSucceeds_Test()
        {
            const int threadCount = 20;
            var endpoint = new Endpoint("concurrent-ep", "/concurrent", typeof(EndpointOneHandler));

            var successCount = 0;
            var exceptionCount = 0;
            var barrier = new Barrier(threadCount);
            var tasks = new Task[threadCount];

            for (var t = 0; t < threadCount; t++)
            {
                tasks[t] = Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    try
                    {
                        endpoint.SetExecutor(new StubExecutor());
                        Interlocked.Increment(ref successCount);
                    }
                    catch (InvalidOperationException)
                    {
                        Interlocked.Increment(ref exceptionCount);
                    }
                });
            }

            Task.WaitAll(tasks);

            Assert.AreEqual(1, successCount,
                "Exactly one concurrent SetExecutor call must succeed");
            Assert.AreEqual(threadCount - 1, exceptionCount,
                "All other concurrent calls must throw InvalidOperationException");
            Assert.IsNotNull(endpoint.Executor,
                "Executor must be non-null after the winning thread sets it");
        }
    }
}
