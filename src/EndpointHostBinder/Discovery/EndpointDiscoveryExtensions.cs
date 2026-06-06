// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2026-03-20 15:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-20 20:44
// ***********************************************************************
//  <copyright file="EndpointDiscoveryExtensions.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.Extensions.DependencyInjection;
using RzR.Extensions.Domain.Primitives;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Attributes;
using RzR.Infrastructure.EndpointHosting.Execution;
using RzR.Infrastructure.EndpointHosting.Models;
using System.Linq;
using System.Reflection;

#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

#endregion

namespace RzR.Infrastructure.EndpointHosting.Discovery
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Provides extension methods for automatic discovery and registration of
    ///     <see cref="IEndpointHostRequestHandler"/> implementations from an assembly.
    /// </summary>
    /// =================================================================================================
    public static class EndpointDiscoveryExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Scans the given <paramref name="assembly"/> for all concrete, non-abstract types that
        ///     implement <see cref="IEndpointHostRequestHandler"/> and are decorated with
        ///     <see cref="EndpointHostAttribute"/>, then registers each discovered handler as a
        ///     transient service and its corresponding <see cref="Endpoint"/> metadata as a singleton.
        /// </summary>
        /// <remarks>
        ///     Only types that carry an <see cref="EndpointHostAttribute"/> are registered.
        ///     Handlers that were already registered in <paramref name="services"/> (e.g. via
        ///     <c>AddHostEndpoint&lt;T&gt;()</c>) are skipped, so it is safe to call this method
        ///     alongside manual registrations without producing duplicates.
        ///     <para>
        ///         Trim/AOT limitation: this method calls <see cref="Assembly.GetTypes"/> and reflects
        ///         over all discovered types. It is not trim-safe. When publishing with IL trimming or
        ///         Native AOT, use <c>AddHostEndpoint&lt;T&gt;()</c> for each handler instead.
        ///     </para>
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the endpoints into.</param>
        /// <param name="assembly">
        ///     The assembly to scan for <see cref="IEndpointHostRequestHandler"/> implementations.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(
            "This method calls Assembly.GetTypes() which is not trim-safe. " +
            "Use AddHostEndpoint<T>() for each handler when publishing with IL trimming or Native AOT.")]
#endif
        public static IServiceCollection AddHostEndpointsFromAssembly(
            this IServiceCollection services, Assembly assembly)
            => AddHostEndpointsFromAssembly(services, assembly, ServiceLifetime.Transient);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Scans the given <paramref name="assembly"/> for all concrete, non-abstract types that
        ///     implement <see cref="IEndpointHostRequestHandler"/> and are decorated with
        ///     <see cref="EndpointHostAttribute"/>, then registers each discovered handler with the
        ///     specified DI <paramref name="handlerLifetime"/> and its corresponding
        ///     <see cref="Endpoint"/> metadata as a singleton.
        /// </summary>
        /// <remarks>
        ///     Only types that carry an <see cref="EndpointHostAttribute"/> are registered.
        ///     Handlers that were already registered in <paramref name="services"/> (e.g. via
        ///     <c>AddHostEndpoint&lt;T&gt;()</c>) are skipped, so it is safe to call this method
        ///     alongside manual registrations without producing duplicates.
        ///     <para>
        ///         Trim/AOT limitation: this method calls <see cref="Assembly.GetTypes"/> and reflects
        ///         over all discovered types. It is not trim-safe. When publishing with IL trimming or
        ///         Native AOT, use <c>AddHostEndpoint&lt;T&gt;()</c> for each handler instead.
        ///     </para>
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the endpoints into.</param>
        /// <param name="assembly">
        ///     The assembly to scan for <see cref="IEndpointHostRequestHandler"/> implementations.
        /// </param>
        /// <param name="handlerLifetime">
        ///     The <see cref="ServiceLifetime"/> applied to every discovered handler. Defaults to
        ///     <see cref="ServiceLifetime.Transient"/> when calling the parameterless overload.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
#if NET5_0_OR_GREATER
        [RequiresUnreferencedCode(
            "This method calls Assembly.GetTypes() which is not trim-safe. " +
            "Use AddHostEndpoint<T>() for each handler when publishing with IL trimming or Native AOT.")]
#endif
        public static IServiceCollection AddHostEndpointsFromAssembly(
            this IServiceCollection services, Assembly assembly, ServiceLifetime handlerLifetime)
        {
            var endpointTypes = assembly
                .GetTypes()
                .Where(x =>
                    x.IsClass.IsTrue() &&
                    x.IsAbstract.IsFalse() &&
                    typeof(IEndpointHostRequestHandler).IsAssignableFrom(x))
                .Select(x => new
                {
                    Type = x,
                    Attribute = x.GetCustomAttribute<EndpointHostAttribute>()
                })
                .Where(x => x.Attribute.IsNotNull());

            foreach (var endpointType in endpointTypes)
            {
                var alreadyRegistered = services.Any(sd =>
                    sd.ServiceType == typeof(Endpoint) &&
                    sd.ImplementationInstance is Endpoint ep &&
                    ep.EndpointType == endpointType.Type);

                if (alreadyRegistered.IsTrue())
                    continue;

                services.Add(new ServiceDescriptor(endpointType.Type, endpointType.Type, handlerLifetime));

                var executor = CompiledEndpointExecutorFactory.CreateTask(endpointType.Type);

                var attr = endpointType.Attribute;
                services.AddSingleton(new Endpoint(
                    attr.Name,
                    attr.Path,
                    endpointType.Type,
                    attr.IsActive,
                    attr.HttpMethods,
                    executor));
            }

            return services;
        }
    }
}