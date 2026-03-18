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

using DomainCommonExtensions.ArraysExtensions;
using DomainCommonExtensions.CommonExtensions;
using DomainCommonExtensions.DataTypeExtensions;
using EndpointHostBinder.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

#endregion

namespace EndpointHostBinder.Models
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
        public IEnumerable<string> AllowedMethods { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="Endpoint" /> class.
        /// </summary>
        /// <param name="name">The endpoint name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="type">The handler type.</param>
        /// =================================================================================================
        public Endpoint(string name, PathString path, Type type)
            : this(name, path, type, true, null) { }

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
        ///     Initializes a new instance of the <see cref="Endpoint" /> class with HTTP method constraints.
        /// </summary>
        /// <param name="name">The endpoint name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="type">The handler type.</param>
        /// <param name="allowedMethods">Allowed HTTP methods (e.g. "GET", "POST"). Pass null or empty to allow all methods.</param>
        /// =================================================================================================
        public Endpoint(string name, PathString path, Type type, string[] allowedMethods)
            : this(name, path, type, true, allowedMethods) { }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="Endpoint" /> class with HTTP method constraints.
        /// </summary>
        /// <param name="name">The endpoint name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="type">The handler type.</param>
        /// <param name="isActive">True if the endpoint is active, false if not.</param>
        /// <param name="allowedMethods">Allowed HTTP methods (e.g. "GET", "POST"). Pass null or empty to allow all methods.</param>
        /// =================================================================================================
        public Endpoint(string name, PathString path, Type type, bool isActive, string[] allowedMethods)
        {
            if (name.IsMissing())
                throw new ArgumentException("Endpoint name cannot be null, empty, or whitespace.", nameof(name));
            if (type.IsNull())
                throw new ArgumentNullException(nameof(type));
            if (!typeof(IEndpointHostRequestHandler).IsAssignableFrom(type))
                throw new ArgumentException($"Type '{type.Name}' must implement {nameof(IEndpointHostRequestHandler)}.", nameof(type));

            Name = name;
            Path = path;
            EndpointType = type;
            IsActive = isActive;
            AllowedMethods = allowedMethods.IsNotNullOrEmptyEnumerable()
                ? new List<string>(allowedMethods)
                : null;
        }
    }
}