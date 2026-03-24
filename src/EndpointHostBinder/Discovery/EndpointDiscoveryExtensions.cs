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

using DomainCommonExtensions.CommonExtensions;
using DomainCommonExtensions.DataTypeExtensions;
using EndpointHostBinder.Abstractions;
using EndpointHostBinder.Attributes;
using EndpointHostBinder.Execution;
using EndpointHostBinder.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

#endregion

namespace EndpointHostBinder.Discovery
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
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the endpoints into.</param>
        /// <param name="assembly">
        ///     The assembly to scan for <see cref="IEndpointHostRequestHandler"/> implementations.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpointsFromAssembly(
            this IServiceCollection services, Assembly assembly)
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
                services.AddTransient(endpointType.Type);

                var executor = CompiledEndpointExecutorFactory.CreateTask(endpointType.Type);

                services.AddSingleton(new Endpoint(
                    endpointType.Attribute.Name,
                    endpointType.Attribute.Path,
                    endpointType.Type,
                    endpointType.Attribute.IsActive,
                    endpointType.Attribute.HttpMethods,
                    executor));
            }

            return services;
        }
    }
}