﻿// <copyright file="IDirectoryCreationOptions.cs" company="Stormpath, Inc.">
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

using Stormpath.SDK.Provider;
using Stormpath.SDK.Resource;

namespace Stormpath.SDK.Directory
{
    /// <summary>
    /// Represents options for an <see cref="IDirectory">Directory</see> creation request.
    /// </summary>
    public interface IDirectoryCreationOptions : ICreationOptions
    {
        /// <summary>
        /// Gets the Provider to create this directory for.
        /// </summary>
        /// <value>The Provider to create this directory for.</value>
        IProvider Provider { get; }
    }
}
