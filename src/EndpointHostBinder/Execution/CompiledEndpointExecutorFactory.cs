// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2026-03-20 15:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-20 19:51
// ***********************************************************************
//  <copyright file="CompiledEndpointExecutorFactory.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using DomainCommonExtensions.CommonExtensions;
using EndpointHostBinder.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointHostBinder.Execution
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A factory that builds pre-compiled, delegate-based <see cref="ICompiledEndpointExecutor"/>
    ///     instances for <see cref="IEndpointHostRequestHandler"/> implementations. By compiling
    ///     LINQ expression trees at registration time, the factory eliminates repeated reflection
    ///     overhead at request dispatch time.
    /// </summary>
    /// =================================================================================================
    internal static class CompiledEndpointExecutorFactory
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Creates an <see cref="ICompiledEndpointExecutor"/> that dispatches requests via the
        ///     asynchronous <see cref="IEndpointHostRequestHandler.RequestProcessAsync"/> path.
        /// </summary>
        /// <param name="handlerType">
        ///     The concrete <see cref="IEndpointHostRequestHandler"/> type to compile a delegate for.
        /// </param>
        /// <returns>
        ///     A compiled <see cref="ICompiledEndpointExecutor"/> backed by an async delegate.
        /// </returns>
        /// =================================================================================================
        public static ICompiledEndpointExecutor CreateTask(Type handlerType)
        {
            var compiled = BuildExecutorTask(handlerType);

            return new CompiledEndpointExecutor(compiled);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Creates an <see cref="ICompiledEndpointExecutor"/> that dispatches requests via the
        ///     synchronous <see cref="IEndpointHostRequestHandler.RequestProcess"/> path.
        /// </summary>
        /// <param name="handlerType">
        ///     The concrete <see cref="IEndpointHostRequestHandler"/> type to compile a delegate for.
        /// </param>
        /// <returns>
        ///     A compiled <see cref="ICompiledEndpointExecutor"/> backed by a synchronous delegate.
        /// </returns>
        /// =================================================================================================
        public static ICompiledEndpointExecutor Create(Type handlerType)
        {
            var compiled = BuildExecutor(handlerType);

            return new CompiledEndpointExecutor(compiled);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds executor task.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="handlerType">Type of the handler.</param>
        /// <returns>
        ///     A function delegate that yields a Task&lt;IEndpointHostResult&gt;
        /// </returns>
        /// =================================================================================================
        private static Func<HttpContext, IServiceProvider, Task<IEndpointHostResult>> BuildExecutorTask(Type handlerType)
        {
            var contextParameter = Expression.Parameter(typeof(HttpContext), "context");
            var servicesParameter = Expression.Parameter(typeof(IServiceProvider), "services");

            var getRequiredServiceMethod = typeof(ServiceProviderServiceExtensions)
                .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), new[] { typeof(IServiceProvider) })
                ?.MakeGenericMethod(handlerType);

            if (getRequiredServiceMethod.IsNull())
                throw new InvalidOperationException("Could not resolve GetRequiredService<T> method.");

            var handlerExpression = Expression.Call(getRequiredServiceMethod!, servicesParameter);

            var handlerInterface = typeof(IEndpointHostRequestHandler);

            var requestProcessAsyncMethod = handlerInterface.GetMethod(
                nameof(IEndpointHostRequestHandler.RequestProcessAsync),
                new[] { typeof(HttpContext), typeof(CancellationToken) });

            if (requestProcessAsyncMethod.IsNull())
                throw new InvalidOperationException("Could not resolve RequestProcessAsync method.");

            var interfaceCast = Expression.Convert(handlerExpression, handlerInterface);
            var requestAbortedExpr = Expression.Property(contextParameter, nameof(HttpContext.RequestAborted));
            var executeCall = Expression.Call(interfaceCast, requestProcessAsyncMethod!, contextParameter, requestAbortedExpr);

            var lambda = Expression.Lambda<Func<HttpContext, IServiceProvider, Task<IEndpointHostResult>>>(
                executeCall,
                contextParameter,
                servicesParameter);

            return lambda.Compile();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Builds an executor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="handlerType">Type of the handler.</param>
        /// <returns>
        ///     A function delegate that yields an IEndpointHostResult.
        /// </returns>
        /// =================================================================================================
        private static Func<HttpContext, IServiceProvider, IEndpointHostResult> BuildExecutor(Type handlerType)
        {
            var contextParameter = Expression.Parameter(typeof(HttpContext), "context");
            var servicesParameter = Expression.Parameter(typeof(IServiceProvider), "services");

            var getRequiredServiceMethod = typeof(ServiceProviderServiceExtensions)
                .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), new[] { typeof(IServiceProvider) })
                ?.MakeGenericMethod(handlerType);

            if (getRequiredServiceMethod.IsNull())
                throw new InvalidOperationException("Could not resolve GetRequiredService<T> method.");

            var handlerExpression = Expression.Call(getRequiredServiceMethod!, servicesParameter);

            var handlerInterface = typeof(IEndpointHostRequestHandler);

            var requestProcessMethod = handlerInterface.GetMethod(
                nameof(IEndpointHostRequestHandler.RequestProcess),
                new[] { typeof(HttpContext) });

            if (requestProcessMethod.IsNull())
                throw new InvalidOperationException("Could not resolve RequestProcess method.");

            var interfaceCast = Expression.Convert(handlerExpression, handlerInterface);
            var executeCall = Expression.Call(interfaceCast, requestProcessMethod!, contextParameter);

            var lambda = Expression.Lambda<Func<HttpContext, IServiceProvider, IEndpointHostResult>>(
                executeCall,
                contextParameter,
                servicesParameter);

            return lambda.Compile();
        }
    }
}