// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2024-04-19 18:43
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 22:57
// ***********************************************************************
//  <copyright file="DependencyInjection.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using EndpointHostBinder.Abstractions;
using EndpointHostBinder.Host;
using EndpointHostBinder.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace EndpointHostBinder
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A dependency injection.
    /// </summary>
    /// =================================================================================================
    public static class DependencyInjection
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An IServiceCollection extension method that registers the endpoint host builder described
        ///     by builder.
        /// </summary>
        /// <param name="builder">The builder to act on.</param>
        /// <returns>
        ///     An IServiceCollection.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection RegisterEndpointHostBuilder(this IServiceCollection builder)
        {
            builder.AddTransient<IEndpointHostRouter, EndpointHostRouter>();

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An IApplicationBuilder extension method that use endpoint host builder.
        /// </summary>
        /// <param name="builder">The builder to act on.</param>
        /// <returns>
        ///     An IApplicationBuilder.
        /// </returns>
        /// =================================================================================================
        public static IApplicationBuilder UseEndpointHostBuilder(this IApplicationBuilder builder)
            => builder.UseMiddleware<EndpointHostBinderMiddleware>();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An IServiceCollection extension method that adds a host endpoint.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="builder">The builder to act on.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <returns>
        ///     An IServiceCollection.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder, string name, string path)
            where T : class, IEndpointHostRequestHandler
        {
            builder.AddTransient<T>();
            builder.AddSingleton(new Endpoint(name, path, typeof(T)));

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An IServiceCollection extension method that adds a host endpoint.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="builder">The builder to act on.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="isActive">True if is active, false if not.</param>
        /// <returns>
        ///     An IServiceCollection.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder, string name, string path, bool isActive)
            where T : class, IEndpointHostRequestHandler
        {
            builder.AddTransient<T>();
            builder.AddSingleton(new Endpoint(name, path, typeof(T), isActive));

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An IServiceCollection extension method that adds a host endpoint.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="builder">The builder to act on.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <returns>
        ///     An IServiceCollection.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder, Endpoint endpoint)
            where T : class, IEndpointHostRequestHandler
        {
            builder.AddTransient<T>();
            builder.AddSingleton(endpoint);

            return builder;
        }
    }
}