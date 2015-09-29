﻿// <copyright file="DefaultEmailVerificationRequest.cs" company="Stormpath, Inc.">
//      Copyright (c) 2015 Stormpath, Inc.
// </copyright>
// <remarks>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </remarks>

using System.Collections.Generic;
using Stormpath.SDK.Account;
using Stormpath.SDK.Impl.DataStore;
using Stormpath.SDK.Impl.Resource;

namespace Stormpath.SDK.Impl.Account
{
    internal sealed class DefaultEmailVerificationRequest : AbstractResource, IEmailVerificationRequest
    {
        private static readonly string LoginPropertyName = "login";

        public DefaultEmailVerificationRequest(IInternalDataStore dataStore)
            : base(dataStore)
        {
        }

        public DefaultEmailVerificationRequest(IInternalDataStore dataStore, IDictionary<string, object> properties)
            : base(dataStore, properties)
        {
        }

        string IEmailVerificationRequest.Login
            => this.GetProperty<string>(LoginPropertyName);

        public IEmailVerificationRequest SetLogin(string usernameOrEmail)
        {
            this.SetProperty(LoginPropertyName, usernameOrEmail);
            return this;
        }
    }
}