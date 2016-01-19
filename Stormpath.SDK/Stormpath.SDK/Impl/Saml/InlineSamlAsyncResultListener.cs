﻿// <copyright file="InlineSamlAsyncResultListener.cs" company="Stormpath, Inc.">
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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.SDK.IdSite;
using Stormpath.SDK.Saml;

namespace Stormpath.SDK.Impl.Saml
{
    internal sealed class InlineSamlAsyncResultListener : ISamlAsyncResultListener
    {
        private readonly Func<IAccountResult, CancellationToken, Task> onAuthenticated;
        private readonly Func<IAccountResult, CancellationToken, Task> onLogout;

        public InlineSamlAsyncResultListener(
            Func<IAccountResult, CancellationToken, Task> onAuthenticated,
            Func<IAccountResult, CancellationToken, Task> onLogout)
        {
            this.onAuthenticated = onAuthenticated;
            this.onLogout = onLogout;
        }

        Task ISamlAsyncResultListener.OnAuthenticatedAsync(IAccountResult result, CancellationToken cancellationToken)
        {
            return this.onAuthenticated != null
                ? this.onAuthenticated(result, cancellationToken)
                : Task.FromResult(true);
        }

        Task ISamlAsyncResultListener.OnLogoutAsync(IAccountResult result, CancellationToken cancellationToken)
        {
            return this.onLogout != null
                ? this.onLogout(result, cancellationToken)
                : Task.FromResult(true);
        }
    }
}
