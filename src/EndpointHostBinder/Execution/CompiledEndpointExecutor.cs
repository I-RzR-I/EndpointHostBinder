// ***********************************************************************
//  Assembly         : RzR.Shared.Services.EndpointHostBinder
//  Author           : RzR
//  Created On       : 2026-03-20 15:03
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-03-20 19:50
// ***********************************************************************
//  <copyright file="CompiledEndpointExecutor.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using RzR.Extensions.Domain.Primitives;
using RzR.Infrastructure.EndpointHosting.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.Infrastructure.EndpointHosting.Execution
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A compiled endpoint executor. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="T:RzR.Infrastructure.EndpointHosting.Abstractions.ICompiledEndpointExecutor"/>
    /// =================================================================================================
    internal sealed class CompiledEndpointExecutor : ICompiledEndpointExecutor
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the executor.
        /// </summary>
        /// =================================================================================================
        private readonly Func<HttpContext, IServiceProvider, Task<IEndpointHostResult>> _executorTask;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the executor.
        /// </summary>
        /// =================================================================================================
        private readonly Func<HttpContext, IServiceProvider, IEndpointHostResult> _executor;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CompiledEndpointExecutor"/> class.
        /// </summary>
        /// <param name="executor">The executor.</param>
        /// =================================================================================================
        public CompiledEndpointExecutor(Func<HttpContext, IServiceProvider, Task<IEndpointHostResult>> executor)
            => _executorTask = executor;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CompiledEndpointExecutor"/> class backed
        ///     by a synchronous handler delegate.
        /// </summary>
        /// <param name="executor">
        ///     A compiled delegate that resolves the handler from the DI container, invokes
        ///     <see cref="IEndpointHostRequestHandler.RequestProcess"/>, and returns the result.
        /// </param>
        /// =================================================================================================
        public CompiledEndpointExecutor(Func<HttpContext, IServiceProvider, IEndpointHostResult> executor)
            => _executor = executor;

        /// <inheritdoc/>
        public async Task ExecuteAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            if (_executorTask.IsNotNull())
            {
                var result = await _executorTask(context, context.RequestServices).ConfigureAwait(false);

                if (result.IsNotNull())
                    await result.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

                return;
            }

            if (_executor.IsNotNull())
            {
                var result = _executor(context, context.RequestServices);

                if (result.IsNotNull())
                    result.Execute(context);

                return;
            }
        }

        /// <inheritdoc/>
        public void Execute(HttpContext context)
        {
            if (_executor.IsNotNull())
            {
                var result = _executor(context, context.RequestServices);

                if (result.IsNotNull())
                    result.Execute(context);

                return;
            }

            if (_executorTask.IsNotNull())
                throw new InvalidOperationException(
                    "This executor was built for asynchronous execution. Use ExecuteAsync instead of Execute.");
        }
    }
}