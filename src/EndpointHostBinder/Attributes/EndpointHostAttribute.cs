// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2026-03-20 15:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-20 19:49
// ***********************************************************************
//  <copyright file="EndpointHostAttribute.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using DomainCommonExtensions.ArraysExtensions;
using System;
using System.Linq;
using System.Net.Http;

#endregion

namespace EndpointHostBinder.Attributes
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Marks a class as an endpoint handler and supplies the routing metadata used by
    ///     <see cref="Discovery.EndpointDiscoveryExtensions.AddHostEndpointsFromAssembly"/> to
    ///     automatically register the handler. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="T:Attribute"/>
    /// =================================================================================================
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EndpointHostAttribute : Attribute
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        /// =================================================================================================
        public string Name { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the URL path this endpoint is mapped to.
        /// </summary>
        /// <value>
        ///     The URL path (e.g. <c>/api/my-endpoint</c>).
        /// </value>
        /// =================================================================================================
        public string Path { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating whether this object is active.
        /// </summary>
        /// <value>
        ///     True if this object is active, false if not.
        /// </value>
        /// =================================================================================================
        public bool IsActive { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the HTTP methods.
        /// </summary>
        /// <value>
        ///     The HTTP methods.
        /// </value>
        /// =================================================================================================
        public HttpMethod[] HttpMethods { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="EndpointHostAttribute"/> class.
        /// </summary>
        /// <param name="name">A display name for the endpoint used in logging and diagnostics.</param>
        /// <param name="path">The URL path the endpoint is mapped to (e.g. <c>/api/my-endpoint</c>).</param>
        /// <param name="isActive">
        ///     (Optional) <see langword="true"/> to enable the endpoint so it handles matching
        ///     requests; <see langword="false"/> to register it as inactive. Defaults to
        ///     <see langword="true"/>.
        /// </param>
        /// <param name="httpMethods">
        ///     (Optional) The HTTP method names this endpoint responds to (e.g. <c>"GET"</c>,
        ///     <c>"POST"</c>). Omit or pass no values to accept all HTTP methods.
        /// </param>
        /// =================================================================================================
        public EndpointHostAttribute(string name, string path, bool isActive = true, params string[] httpMethods)
        {
            Name = name;
            Path = path;
            IsActive = isActive;
            HttpMethods = httpMethods.IsNotNullOrEmptyEnumerable()
                ? httpMethods.Select(m => new HttpMethod(m)).ToArray()
                : [];
        }
    }
}