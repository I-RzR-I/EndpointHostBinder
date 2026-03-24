// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2024-04-19 18:10
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-04-21 22:57
// ***********************************************************************
//  <copyright file="IEndpointHostResult.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace EndpointHostBinder.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Represents the result produced by an <see cref="IEndpointHostRequestHandler"/>, responsible
    ///     for writing the final HTTP response (status code, headers, body) to the current context.
    /// </summary>
    /// =================================================================================================
    public interface IEndpointHostResult
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Asynchronously writes the endpoint result to the HTTP response.
        /// </summary>
        /// <param name="context">The current HTTP context whose response will be populated.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> that completes when the response has been written.
        /// </returns>
        /// =================================================================================================
        Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Synchronously writes the endpoint result to the HTTP response.
        /// </summary>
        /// <param name="context">The current HTTP context whose response will be populated.</param>
        /// =================================================================================================
        void Execute(HttpContext context);
    }
}