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
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Defines the contract for handling an incoming HTTP request routed to a specific endpoint.
    ///     Implementations contain the application logic for processing the request and returning
    ///     an <see cref="IEndpointHostResult"/> that writes the response.
    /// </summary>
    /// =================================================================================================
    public interface IEndpointHostRequestHandler
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Asynchronously processes the incoming HTTP request and returns the result to be written
        ///     to the response.
        /// </summary>
        /// <param name="context">The current HTTP context for the request being handled.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> whose result is an <see cref="IEndpointHostResult"/>
        ///     that will be executed to write the HTTP response.
        /// </returns>
        /// =================================================================================================
        Task<IEndpointHostResult> RequestProcessAsync(HttpContext context, CancellationToken cancellationToken = default);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Synchronously processes the incoming HTTP request and returns the result to be written
        ///     to the response.
        /// </summary>
        /// <param name="context">The current HTTP context for the request being handled.</param>
        /// <returns>
        ///     An <see cref="IEndpointHostResult"/> that will be executed to write the HTTP response.
        /// </returns>
        /// =================================================================================================
        IEndpointHostResult RequestProcess(HttpContext context);
    }
}