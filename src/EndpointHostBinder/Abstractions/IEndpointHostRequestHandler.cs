// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2024-04-19 18:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 22:57
// ***********************************************************************
//  <copyright file="IEndpointHostRequestHandler.cs" company="">
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
    ///     Interface for endpoint host handler.
    /// </summary>
    /// =================================================================================================
    public interface IEndpointHostRequestHandler
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Request process asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     The request process.
        /// </returns>
        /// =================================================================================================
        Task<IEndpointHostResult> RequestProcessAsync(HttpContext context);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Request process.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>
        ///     An IEndpointHostResult.
        /// </returns>
        /// =================================================================================================
        IEndpointHostResult RequestProcess(HttpContext context);
    }
}