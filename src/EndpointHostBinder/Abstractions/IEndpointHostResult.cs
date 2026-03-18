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
    ///     Interface for endpoint host result.
    /// </summary>
    /// =================================================================================================
    public interface IEndpointHostResult
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Executes the 'asynchronous' operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Executes the given context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// =================================================================================================
        void Execute(HttpContext context);
    }
}