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

using DomainCommonExtensions.DataTypeExtensions;
using EndpointHostBinder.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

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
        ///     Initializes a new instance of the <see cref="Endpoint" /> class.
        /// </summary>
        /// <param name="name">The endpoint name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="type">The type.</param>
        /// =================================================================================================
        public Endpoint(string name, PathString path, Type type)
        {
            if (type.GetInterfaces().Any(x => x.Name == nameof(IEndpointHostRequestHandler)).IsFalse())
                throw new Exception("Endpoint type is not typeof(IEndpointHostHandler)");

            Name = name;
            Path = path;
            EndpointType = type;
            IsActive = true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="Endpoint" /> class.
        /// </summary>
        /// <param name="name">The endpoint name.</param>
        /// <param name="path">Full pathname of the resource.</param>
        /// <param name="type">The type.</param>
        /// <param name="isActive">True if is active, false if not.</param>
        /// =================================================================================================
        public Endpoint(string name, PathString path, Type type, bool isActive)
        {
            if (type.GetInterfaces().Any(x => x.Name == nameof(IEndpointHostRequestHandler)).IsFalse())
                throw new Exception("Endpoint type is not typeof(IEndpointHostHandler)");

            Name = name;
            Path = path;
            EndpointType = type;
            IsActive = isActive;
        }
    }
}