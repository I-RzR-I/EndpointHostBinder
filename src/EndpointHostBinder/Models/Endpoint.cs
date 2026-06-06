// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2024-04-19 18:38
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 22:57
// ***********************************************************************
//  <copyright file="Endpoint.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using RzR.Extensions.Domain.Collections;
using RzR.Extensions.Domain.Primitives;
using RzR.Extensions.Domain.Text;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An endpoint.
    /// </summary>
    /// =================================================================================================
    public class Endpoint
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the endpoint name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        /// =================================================================================================
        public string Name { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the full pathname of the resource.
        /// </summary>
        /// <value>
        ///     The full pathname of the resource.
        /// </value>
        /// =================================================================================================
        public PathString Path { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating whether this object is active.
        /// </summary>
        /// <value>
        ///     True if this object is active, false if not.
        /// </value>
        /// =================================================================================================
        public bool IsActive { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the type of the endpoint.
        /// </summary>
        /// <value>
        ///     The type of the endpoint.
        /// </value>
        /// =================================================================================================
        public Type EndpointType { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the HTTP methods allowed for this endpoint. When <see langword="null" /> or empty, all
        ///     HTTP methods are accepted.
        /// </summary>
        /// =================================================================================================
        public IEnumerable<HttpMethod> AllowedMethods { get; private set; }

        private ICompiledEndpointExecutor _executor;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the executor.
        /// </summary>
        /// <value>
        ///     The executor.
        /// </value>
        /// =================================================================================================
        public ICompiledEndpointExecutor Executor => _executor;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="Endpoint" /> class.
        /// </summary>
        /// <param name="name">The endpoint name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="type">The handler type.</param>
        /// <param name="isActive">True if the endpoint is active, false if not.</param>
        /// =================================================================================================
        public Endpoint(string name, PathString path, Type type, bool isActive)
            : this(name, path, type, isActive, null) { }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="Endpoint" /> class with HTTP method
        ///     constraints.
        /// </summary>
        /// <param name="name">The endpoint name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="type">The handler type.</param>
        /// <param name="allowedMethods">
        ///     Allowed HTTP methods (e.g. <see cref="HttpMethod.Get"/>, <see cref="HttpMethod.Post"/>).
        ///     Pass null or empty to allow all methods.
        /// </param>
        /// =================================================================================================
        public Endpoint(string name, PathString path, Type type, HttpMethod[] allowedMethods)
            : this(name, path, type, true, allowedMethods) { }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="Endpoint" /> class with HTTP method
        ///     constraints.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="name">The endpoint name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="type">The handler type.</param>
        /// <param name="isActive">True if the endpoint is active, false if not.</param>
        /// <param name="allowedMethods">
        ///     Allowed HTTP methods (e.g. <see cref="HttpMethod.Get"/>, <see cref="HttpMethod.Post"/>).
        ///     Pass null or empty to allow all methods.
        /// </param>
        /// =================================================================================================
        public Endpoint(string name, PathString path, Type type, bool isActive = true, HttpMethod[] allowedMethods = null)
        {
            if (name.IsMissing())
                throw new ArgumentException("Endpoint name cannot be null, empty, or whitespace.", nameof(name));
            if (type.IsNull())
                throw new ArgumentNullException(nameof(type));
            if (typeof(IEndpointHostRequestHandler).IsAssignableFrom(type).IsFalse())
                throw new ArgumentException($"Type '{type.Name}' must implement {nameof(IEndpointHostRequestHandler)}.", nameof(type));

            Name = name;
            Path = path;
            EndpointType = type;
            IsActive = isActive;
            AllowedMethods = allowedMethods.IsNotNullOrEmptyEnumerable()
                ? allowedMethods
                : null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="Endpoint" /> class with HTTP method
        ///     constraints.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="name">The endpoint name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="type">The handler type.</param>
        /// <param name="isActive">True if the endpoint is active, false if not.</param>
        /// <param name="allowedMethods">
        ///     Allowed HTTP methods (e.g. <see cref="HttpMethod.Get"/>, <see cref="HttpMethod.Post"/>).
        ///     Pass null or empty to allow all methods.
        /// </param>
        /// <param name="executor">The executor.</param>
        /// =================================================================================================
        public Endpoint(string name, PathString path, Type type, bool isActive, HttpMethod[] allowedMethods,
            ICompiledEndpointExecutor executor)
        {
            if (name.IsMissing())
                throw new ArgumentException("Endpoint name cannot be null, empty, or whitespace.", nameof(name));
            if (type.IsNull())
                throw new ArgumentNullException(nameof(type));
            if (typeof(IEndpointHostRequestHandler).IsAssignableFrom(type).IsFalse())
                throw new ArgumentException($"Type '{type.Name}' must implement {nameof(IEndpointHostRequestHandler)}.", nameof(type));

            Name = name;
            Path = path;
            EndpointType = type;
            IsActive = isActive;
            _executor = executor;
            AllowedMethods = allowedMethods.IsNotNullOrEmptyEnumerable()
                ? allowedMethods
                : null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Sets the executor for this endpoint using an atomic compare-and-swap that succeeds
        ///     exactly once. The operation is safe against concurrent callers: the first caller whose
        ///     compare-and-swap wins assigns the executor; all subsequent callers — whether sequential
        ///     or concurrent — receive an <see cref="InvalidOperationException" />.
        /// </summary>
        /// <param name="executor">The compiled executor to assign.</param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when <see cref="Executor"/> is already non-null.
        /// </exception>
        /// =================================================================================================
        public void SetExecutor(ICompiledEndpointExecutor executor)
        {
            var prior = Interlocked.CompareExchange(ref _executor, executor, null);
            if (prior.IsNotNull())
                throw new InvalidOperationException(
                    $"Executor for endpoint '{Name}' has already been set and cannot be replaced. " +
                    "Assign the executor once, either via the constructor or a single SetExecutor call.");
        }
    }
}