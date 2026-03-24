// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2024-04-19 18:12
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 22:57
// ***********************************************************************
//  <copyright file="IEndpointHostRouter.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using EndpointHostBinder.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

#endregion

namespace EndpointHostBinder.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Defines the contract for matching an incoming HTTP request to a registered
    ///     <see cref="Endpoint"/> by comparing the request path and HTTP method against the
    ///     collection of registered, active endpoints.
    /// </summary>
    /// =================================================================================================
    public interface IEndpointHostRouter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Finds the registered <see cref="Endpoint"/> whose path and allowed HTTP methods match
        ///     the current request.
        /// </summary>
        /// <param name="context">The current HTTP context containing the request to match.</param>
        /// <returns>
        ///     The matching active <see cref="Endpoint"/> if one is found; <see langword="null"/> if no
        ///     endpoint matches the request path and HTTP method, or if the matched endpoint is
        ///     currently inactive.
        /// </returns>
        /// =================================================================================================
        Endpoint Find(HttpContext context);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Determines whether a registered, active endpoint matches the path and HTTP method of
        ///     the incoming request.
        /// </summary>
        /// <param name="context">The current HTTP context containing the request to check.</param>
        /// <returns>
        ///     <see langword="true"/> if a matching active endpoint exists; otherwise
        ///     <see langword="false"/>.
        /// </returns>
        /// =================================================================================================
        bool Exist(HttpContext context);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Asynchronously determines whether a registered, active endpoint matches the path and
        ///     HTTP method of the incoming request.
        /// </summary>
        /// <param name="context">The current HTTP context containing the request to check.</param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> whose result is <see langword="true"/> if a matching
        ///     active endpoint exists; otherwise <see langword="false"/>.
        /// </returns>
        /// =================================================================================================
        Task<bool> ExistAsync(HttpContext context);
    }
}