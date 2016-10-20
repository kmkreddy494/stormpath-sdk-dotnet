﻿// <copyright file="FactorStatus.cs" company="Stormpath, Inc.">
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

using Stormpath.SDK.Resource;

namespace Stormpath.SDK.Account
{
    /// <summary>
    /// Represents the various states a <see cref="IFactor">Factor</see> may be in.
    /// </summary>
    public sealed class FactorStatus : AbstractEnumProperty
    {
        /// <summary>
        /// The factor is enabled.
        /// </summary>
        public static FactorStatus Enabled = new FactorStatus("ENABLED");

        /// <summary>
        /// The factor is disabled.
        /// </summary>
        public static FactorStatus Disabled = new FactorStatus("DISABLED");

        /// <summary>
        /// Creates a new <see cref="FactorStatus"/> instance.
        /// </summary>
        /// <param name="value">The value to use.</param>
        public FactorStatus(string value)
            : base(value)
        {
        }
    }
}