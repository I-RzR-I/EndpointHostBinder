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

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

#endregion

namespace EndpointHostBinder.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Interface for endpoint host router.
    /// </summary>
    /// =================================================================================================
    public interface IEndpointHostRouter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Searches for the first match for the given HTTP context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     An IEndpointHostHandler.
        /// </returns>
        /// =================================================================================================
        IEndpointHostRequestHandler Find(HttpContext context);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Exists the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        /// =================================================================================================
        bool Exist(HttpContext context);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Exist asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     The exist.
        /// </returns>
        /// =================================================================================================
        Task<bool> ExistAsync(HttpContext context);
    }
}