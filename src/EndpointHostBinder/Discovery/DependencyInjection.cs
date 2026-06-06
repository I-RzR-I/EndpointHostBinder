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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RzR.Extensions.Domain.Primitives;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using RzR.Infrastructure.EndpointHosting.Configuration;
using RzR.Infrastructure.EndpointHosting.Execution;
using RzR.Infrastructure.EndpointHosting.Host;
using RzR.Infrastructure.EndpointHosting.Models;
using System;
using System.Net.Http;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Discovery
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Provides extension methods for registering the endpoint host infrastructure and individual
    ///     <see cref="IEndpointHostRequestHandler"/> implementations into the ASP.NET Core
    ///     dependency-injection container and middleware pipeline.
    /// </summary>
    /// =================================================================================================
    public static class DependencyInjection
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers the core endpoint host infrastructure into the DI container using default
        ///     <see cref="EndpointHostOptions"/>. Specifically, registers
        ///     <see cref="EndpointHostRouter"/> as the <see cref="IEndpointHostRouter"/> singleton
        ///     that resolves incoming requests to their matching <see cref="Models.Endpoint"/>.
        /// </summary>
        /// <remarks>
        ///     This method must be called before <see cref="UseEndpointHostBuilder"/>. It is also a
        ///     prerequisite for any <c>AddHostEndpoint</c> registration to function correctly.
        /// </remarks>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection RegisterEndpointHostBuilder(this IServiceCollection builder)
            => RegisterEndpointHostBuilder(builder, _ => { });

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers the core endpoint host infrastructure into the DI container with a custom
        ///     <see cref="EndpointHostOptions"/> configuration. Specifically, registers
        ///     <see cref="EndpointHostRouter"/> as the <see cref="IEndpointHostRouter"/> singleton
        ///     that resolves incoming requests to their matching <see cref="Models.Endpoint"/>.
        /// </summary>
        /// <remarks>
        ///     This method must be called before <see cref="UseEndpointHostBuilder"/>. It is also a
        ///     prerequisite for any <c>AddHostEndpoint</c> registration to function correctly.
        /// </remarks>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="configure">
        ///     A delegate that configures the <see cref="EndpointHostOptions"/> before the
        ///     infrastructure is registered.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection RegisterEndpointHostBuilder(this IServiceCollection builder,
            Action<EndpointHostOptions> configure)
        {
            if (configure.IsNull())
                throw new ArgumentNullException(nameof(configure));

            var options = new EndpointHostOptions();
            configure(options);

            builder.AddSingleton(options);
            builder.AddSingleton<IEndpointHostRouter, EndpointHostRouter>();

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Inserts <see cref="EndpointHostBinderMiddleware"/> into the ASP.NET Core request
        ///     pipeline. The middleware intercepts every request, checks whether it matches a
        ///     registered endpoint, and — if so — dispatches it to the compiled handler executor.
        ///     Unmatched requests are forwarded to the next middleware.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/> to configure.</param>
        /// <returns>
        ///     The same <see cref="IApplicationBuilder"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IApplicationBuilder UseEndpointHostBuilder(this IApplicationBuilder builder)
            => builder.UseMiddleware<EndpointHostBinderMiddleware>();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> that responds to any HTTP
        ///     method. <typeparamref name="T"/> is registered as a transient service and a compiled
        ///     <see cref="Models.Endpoint"/> singleton is added for routing.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="name">A display name for the endpoint used in logging.</param>
        /// <param name="path">The URL path the endpoint is mapped to (e.g. <c>/api/my-endpoint</c>).</param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder,
            string name, string path)
            where T : class, IEndpointHostRequestHandler
            => AddHostEndpoint<T>(builder, name, path, ServiceLifetime.Transient);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> that responds to any HTTP
        ///     method, using the specified DI <paramref name="lifetime"/> for the handler.
        ///     A compiled <see cref="Models.Endpoint"/> singleton is always added for routing.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="name">A display name for the endpoint used in logging.</param>
        /// <param name="path">The URL path the endpoint is mapped to (e.g. <c>/api/my-endpoint</c>).</param>
        /// <param name="lifetime">
        ///     The <see cref="ServiceLifetime"/> used when registering the handler. Defaults to
        ///     <see cref="ServiceLifetime.Transient"/> to preserve backward-compatible behaviour.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder,
            string name, string path, ServiceLifetime lifetime)
            where T : class, IEndpointHostRequestHandler
        {
            builder.Add(new ServiceDescriptor(typeof(T), typeof(T), lifetime));

            var executor = CompiledEndpointExecutorFactory.CreateTask(typeof(T));
            builder.AddSingleton(new Endpoint(name, path, typeof(T), true, Array.Empty<HttpMethod>(), executor));

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> with an explicit active/
        ///     inactive flag and no HTTP method restriction.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="name">A display name for the endpoint used in logging.</param>
        /// <param name="path">The URL path the endpoint is mapped to (e.g. <c>/api/my-endpoint</c>).</param>
        /// <param name="isActive">
        ///     <see langword="true"/> to enable the endpoint so it handles matching requests;
        ///     <see langword="false"/> to register it as inactive (requests will fall through).
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder,
            string name, string path, bool isActive)
            where T : class, IEndpointHostRequestHandler
            => AddHostEndpoint<T>(builder, name, path, isActive, ServiceLifetime.Transient);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> with an explicit active/
        ///     inactive flag, no HTTP method restriction, and the specified DI
        ///     <paramref name="lifetime"/> for the handler.
        ///     A compiled <see cref="Models.Endpoint"/> singleton is always added for routing.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="name">A display name for the endpoint used in logging.</param>
        /// <param name="path">The URL path the endpoint is mapped to (e.g. <c>/api/my-endpoint</c>).</param>
        /// <param name="isActive">
        ///     <see langword="true"/> to enable the endpoint so it handles matching requests;
        ///     <see langword="false"/> to register it as inactive (requests will fall through).
        /// </param>
        /// <param name="lifetime">
        ///     The <see cref="ServiceLifetime"/> used when registering the handler. Defaults to
        ///     <see cref="ServiceLifetime.Transient"/> to preserve backward-compatible behaviour.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder,
            string name, string path, bool isActive, ServiceLifetime lifetime)
            where T : class, IEndpointHostRequestHandler
        {
            builder.Add(new ServiceDescriptor(typeof(T), typeof(T), lifetime));

            var executor = CompiledEndpointExecutorFactory.CreateTask(typeof(T));
            builder.AddSingleton(new Endpoint(name, path, typeof(T), isActive, Array.Empty<HttpMethod>(), executor));

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> using a pre-configured
        ///     <see cref="Models.Endpoint"/> instance. If <paramref name="endpoint"/> does not yet have
        ///     a compiled executor assigned, one is built automatically via
        ///     <see cref="CompiledEndpointExecutorFactory"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="endpoint">
        ///     A fully configured <see cref="Models.Endpoint"/> that describes the endpoint's name,
        ///     path, active state, and allowed HTTP methods.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder, Endpoint endpoint)
            where T : class, IEndpointHostRequestHandler
            => AddHostEndpoint<T>(builder, endpoint, ServiceLifetime.Transient);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> using a pre-configured
        ///     <see cref="Models.Endpoint"/> instance and the specified DI <paramref name="lifetime"/>
        ///     for the handler. If <paramref name="endpoint"/> does not yet have a compiled executor
        ///     assigned, one is built automatically via <see cref="CompiledEndpointExecutorFactory"/>.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="endpoint">
        ///     A fully configured <see cref="Models.Endpoint"/> that describes the endpoint's name,
        ///     path, active state, and allowed HTTP methods.
        /// </param>
        /// <param name="lifetime">
        ///     The <see cref="ServiceLifetime"/> used when registering the handler. Defaults to
        ///     <see cref="ServiceLifetime.Transient"/> to preserve backward-compatible behaviour.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder, Endpoint endpoint,
            ServiceLifetime lifetime)
            where T : class, IEndpointHostRequestHandler
        {
            builder.Add(new ServiceDescriptor(typeof(T), typeof(T), lifetime));

            if (endpoint.Executor.IsNull())
                endpoint.SetExecutor(CompiledEndpointExecutorFactory.CreateTask(typeof(T)));

            builder.AddSingleton(endpoint);

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> that responds only to the
        ///     specified HTTP methods. The endpoint is active by default.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="name">A display name for the endpoint used in logging.</param>
        /// <param name="path">The URL path the endpoint is mapped to (e.g. <c>/api/my-endpoint</c>).</param>
        /// <param name="allowedMethods">
        ///     The HTTP methods this endpoint accepts (e.g. <see cref="HttpMethod.Get"/>,
        ///     <see cref="HttpMethod.Post"/>). Pass <see langword="null"/> or an empty array to accept
        ///     all methods.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder,
            string name, string path, HttpMethod[] allowedMethods)
            where T : class, IEndpointHostRequestHandler
            => AddHostEndpoint<T>(builder, name, path, allowedMethods, ServiceLifetime.Transient);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> that responds only to the
        ///     specified HTTP methods, using the specified DI <paramref name="lifetime"/> for the handler.
        ///     The endpoint is active by default. A compiled <see cref="Models.Endpoint"/> singleton is
        ///     always added for routing.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="name">A display name for the endpoint used in logging.</param>
        /// <param name="path">The URL path the endpoint is mapped to (e.g. <c>/api/my-endpoint</c>).</param>
        /// <param name="allowedMethods">
        ///     The HTTP methods this endpoint accepts (e.g. <see cref="HttpMethod.Get"/>,
        ///     <see cref="HttpMethod.Post"/>). Pass <see langword="null"/> or an empty array to accept
        ///     all methods.
        /// </param>
        /// <param name="lifetime">
        ///     The <see cref="ServiceLifetime"/> used when registering the handler. Defaults to
        ///     <see cref="ServiceLifetime.Transient"/> to preserve backward-compatible behaviour.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder,
            string name, string path, HttpMethod[] allowedMethods, ServiceLifetime lifetime)
            where T : class, IEndpointHostRequestHandler
        {
            builder.Add(new ServiceDescriptor(typeof(T), typeof(T), lifetime));

            var executor = CompiledEndpointExecutorFactory.CreateTask(typeof(T));
            builder.AddSingleton(new Endpoint(name, path, typeof(T), true, allowedMethods, executor));

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> with full control over the
        ///     active flag and the set of accepted HTTP methods.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="name">A display name for the endpoint used in logging.</param>
        /// <param name="path">The URL path the endpoint is mapped to (e.g. <c>/api/my-endpoint</c>).</param>
        /// <param name="isActive">
        ///     <see langword="true"/> to enable the endpoint so it handles matching requests;
        ///     <see langword="false"/> to register it as inactive (requests will fall through).
        /// </param>
        /// <param name="allowedMethods">
        ///     The HTTP methods this endpoint accepts (e.g. <see cref="HttpMethod.Get"/>,
        ///     <see cref="HttpMethod.Post"/>). Pass <see langword="null"/> or an empty array to accept
        ///     all methods.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder,
            string name, string path, bool isActive, HttpMethod[] allowedMethods)
            where T : class, IEndpointHostRequestHandler
            => AddHostEndpoint<T>(builder, name, path, isActive, allowedMethods, ServiceLifetime.Transient);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Registers a single endpoint handler <typeparamref name="T"/> with full control over the
        ///     active flag, the set of accepted HTTP methods, and the DI <paramref name="lifetime"/>
        ///     for the handler. A compiled <see cref="Models.Endpoint"/> singleton is always added for
        ///     routing.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IEndpointHostRequestHandler"/> implementation to register.
        /// </typeparam>
        /// <param name="builder">The <see cref="IServiceCollection"/> to register services into.</param>
        /// <param name="name">A display name for the endpoint used in logging.</param>
        /// <param name="path">The URL path the endpoint is mapped to (e.g. <c>/api/my-endpoint</c>).</param>
        /// <param name="isActive">
        ///     <see langword="true"/> to enable the endpoint so it handles matching requests;
        ///     <see langword="false"/> to register it as inactive (requests will fall through).
        /// </param>
        /// <param name="allowedMethods">
        ///     The HTTP methods this endpoint accepts (e.g. <see cref="HttpMethod.Get"/>,
        ///     <see cref="HttpMethod.Post"/>). Pass <see langword="null"/> or an empty array to accept
        ///     all methods.
        /// </param>
        /// <param name="lifetime">
        ///     The <see cref="ServiceLifetime"/> used when registering the handler. Defaults to
        ///     <see cref="ServiceLifetime.Transient"/> to preserve backward-compatible behaviour.
        /// </param>
        /// <returns>
        ///     The same <see cref="IServiceCollection"/> instance so that calls can be chained.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddHostEndpoint<T>(this IServiceCollection builder,
            string name, string path, bool isActive, HttpMethod[] allowedMethods, ServiceLifetime lifetime)
            where T : class, IEndpointHostRequestHandler
        {
            builder.Add(new ServiceDescriptor(typeof(T), typeof(T), lifetime));

            var executor = CompiledEndpointExecutorFactory.CreateTask(typeof(T));
            builder.AddSingleton(new Endpoint(name, path, typeof(T), isActive, allowedMethods, executor));

            return builder;
        }
    }
}