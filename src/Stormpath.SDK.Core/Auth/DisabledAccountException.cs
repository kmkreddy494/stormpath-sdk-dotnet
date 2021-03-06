﻿// <copyright file="DisabledAccountException.cs" company="Stormpath, Inc.">
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

using Stormpath.SDK.Account;

namespace Stormpath.SDK.Auth
{
    /// <summary>
    /// Represents an error that occurs during API Authentication
    /// when the API Key is found, but the owning <see cref="IAccount">Account</see> is disabled.
    /// </summary>
    public sealed class DisabledAccountException : ApiAuthenticationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisabledAccountException"/> class.
        /// </summary>
        /// <param name="status">The account status.</param>
        public DisabledAccountException(AccountStatus status)
            : base($"The account is not enabled (status: {status}).")
        {
        }
    }
}
