﻿// <copyright file="DefaultApplication.Saml.cs" company="Stormpath, Inc.">
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
using Stormpath.SDK.Application;
using Stormpath.SDK.Saml;

namespace Stormpath.SDK.Impl.Application
{
    internal sealed partial class DefaultApplication
    {
        ISamlIdpUrlBuilder IApplication.NewSamlIdpUrlBuilder()
        {
            throw new NotImplementedException();
        }

        Task<ISamlPolicy> IApplication.GetSamlPolicyAsync(CancellationToken cancellationToken)
            => this.GetInternalAsyncDataStore().GetResourceAsync<ISamlPolicy>(this.SamlPolicy.Href, cancellationToken);

        ISamlPolicy IApplicationSync.GetSamlPolicy()
            => this.GetInternalSyncDataStore().GetResource<ISamlPolicy>(this.SamlPolicy.Href);
    }
}
