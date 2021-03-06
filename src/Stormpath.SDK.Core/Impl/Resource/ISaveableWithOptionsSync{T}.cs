﻿// <copyright file="ISaveableWithOptionsSync{T}.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using Stormpath.SDK.Resource;

namespace Stormpath.SDK.Impl.Resource
{
    /// <summary>
    /// Represents a resource that can be saved with additional options.
    /// </summary>
    /// <typeparam name="T">The <see cref="IResource">Resource</see> type.</typeparam>
    internal interface ISaveableWithOptionsSync<T> : ISaveableSync<T>
        where T : IResource
    {
        /// <summary>
        /// Synchronous counterpart to <see cref="ISaveableWithOptions{T}.SaveAsync(Action{IRetrievalOptions{T}}, System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <param name="responseOptions">The options to apply to this request.</param>
        /// <returns>The persisted resource data.</returns>
        T Save(Action<IRetrievalOptions<T>> responseOptions);
    }
}
