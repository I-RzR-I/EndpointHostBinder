// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2026-03-20 15:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-20 19:48
// ***********************************************************************
//  <copyright file="ICompiledEndpointExecutor.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
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
    ///     Defines the contract for a pre-compiled delegate that dispatches an incoming HTTP request
    ///     to its associated <see cref="IEndpointHostRequestHandler"/> and writes the produced
    ///     <see cref="IEndpointHostResult"/> to the response.
    /// </summary>
    /// =================================================================================================
    public interface ICompiledEndpointExecutor
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Asynchronously dispatches the HTTP request to the compiled handler delegate, awaits the
        ///     resulting <see cref="IEndpointHostResult"/>, and writes it to the response.
        /// </summary>
        /// <param name="context">The current HTTP context for the request being processed.</param>
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
        ///     Synchronously dispatches the HTTP request to the compiled handler delegate and writes
        ///     the resulting <see cref="IEndpointHostResult"/> to the response.
        /// </summary>
        /// <param name="context">The current HTTP context for the request being processed.</param>
        /// =================================================================================================
        void Execute(HttpContext context);
    }
}