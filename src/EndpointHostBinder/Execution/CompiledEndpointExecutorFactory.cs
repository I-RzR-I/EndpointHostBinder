// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2026-03-20 15:03
//
//  Last Modified By : RzR
//  Last Modified On : 2026-06-04 23:44
// ***********************************************************************
//  <copyright file="CompiledEndpointExecutorFactory.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
//
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RzR.Extensions.Domain.Primitives;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

#endregion

namespace RzR.Infrastructure.EndpointHosting.Execution
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A factory that builds pre-compiled, delegate-based <see cref="ICompiledEndpointExecutor"/>
    ///     instances for <see cref="IEndpointHostRequestHandler"/> implementations. By compiling
    ///     LINQ expression trees at registration time, the factory eliminates repeated reflection
    ///     overhead at request dispatch time.
    /// </summary>
    /// <remarks>
    ///     Trim/AOT limitation: this factory uses LINQ expression-tree compilation and reflection
    ///     over handler types at registration time. It is not trim-safe or Native-AOT-safe on its own.
    ///     When publishing with trimming or Native AOT, prefer the explicit generic
    ///     <c>AddHostEndpoint&lt;T&gt;()</c> registration path which avoids assembly scanning.
    /// </remarks>
    /// =================================================================================================
    internal static class CompiledEndpointExecutorFactory
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Creates an <see cref="ICompiledEndpointExecutor"/> that dispatches requests via the
        ///     asynchronous <see cref="IEndpointHostRequestHandler.RequestProcessAsync"/> path.
        /// </summary>
        /// <remarks>
        ///     Trim/AOT limitation: this method compiles a LINQ expression tree and reflects over
        ///     <paramref name="handlerType"/> at runtime. Members of the handler type must be
        ///     preserved by the trimmer. Prefer <c>AddHostEndpoint&lt;T&gt;()</c> when targeting
        ///     Native AOT or publishing with IL trimming enabled.
        /// </remarks>
        /// <param name="handlerType">
        ///     The concrete <see cref="IEndpointHostRequestHandler"/> type to compile a delegate for.
        /// </param>
        /// <returns>
        ///     A compiled <see cref="ICompiledEndpointExecutor"/> backed by an async delegate.
        /// </returns>
        /// =================================================================================================
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(
            "This method uses reflection and LINQ expression-tree compilation over the handler type. " +
            "Use the explicit AddHostEndpoint<T>() registration to avoid trim/AOT issues.")]
#endif
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(
            "This method compiles LINQ expression trees at runtime, which is not compatible with " +
            "Native AOT. Use the explicit AddHostEndpoint<T>() registration instead.")]
#endif
        public static ICompiledEndpointExecutor CreateTask(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            Type handlerType)
        {
            var compiled = BuildExecutorTask(handlerType);

            return new CompiledEndpointExecutor(compiled);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Creates an <see cref="ICompiledEndpointExecutor"/> that dispatches requests via the
        ///     synchronous <see cref="IEndpointHostRequestHandler.RequestProcess"/> path.
        /// </summary>
        /// <remarks>
        ///     Trim/AOT limitation: this method compiles a LINQ expression tree and reflects over
        ///     <paramref name="handlerType"/> at runtime. Members of the handler type must be
        ///     preserved by the trimmer. Prefer <c>AddHostEndpoint&lt;T&gt;()</c> when targeting
        ///     Native AOT or publishing with IL trimming enabled.
        /// </remarks>
        /// <param name="handlerType">
        ///     The concrete <see cref="IEndpointHostRequestHandler"/> type to compile a delegate for.
        /// </param>
        /// <returns>
        ///     A compiled <see cref="ICompiledEndpointExecutor"/> backed by a synchronous delegate.
        /// </returns>
        /// =================================================================================================
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(
            "This method uses reflection and LINQ expression-tree compilation over the handler type. " +
            "Use the explicit AddHostEndpoint<T>() registration to avoid trim/AOT issues.")]
#endif
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(
            "This method compiles LINQ expression trees at runtime, which is not compatible with " +
            "Native AOT. Use the explicit AddHostEndpoint<T>() registration instead.")]
#endif
        public static ICompiledEndpointExecutor Create(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            Type handlerType)
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
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(
            "Reflects over handlerType members to build the executor expression tree.")]
#endif
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(
            "Compiles LINQ expression trees at runtime; not compatible with Native AOT.")]
#endif
        private static Func<HttpContext, IServiceProvider, Task<IEndpointHostResult>> BuildExecutorTask(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            Type handlerType)
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
                executeCall, contextParameter, servicesParameter);

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
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(
            "Reflects over handlerType members to build the executor expression tree.")]
#endif
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(
            "Compiles LINQ expression trees at runtime; not compatible with Native AOT.")]
#endif
        private static Func<HttpContext, IServiceProvider, IEndpointHostResult> BuildExecutor(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            Type handlerType)
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
                executeCall, contextParameter, servicesParameter);

            return lambda.Compile();
        }
    }
}
