﻿// <copyright file="DefaultDataStore.Api.cs" company="Stormpath, Inc.">
// Copyright (c) 2015 Stormpath, Inc.
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
using System.Collections.Generic;
using Stormpath.SDK.Api;
using Stormpath.SDK.Cache;
using Stormpath.SDK.DataStore;
using Stormpath.SDK.Impl.Cache;
using Stormpath.SDK.Impl.Http;
using Stormpath.SDK.Impl.Http.Support;
using Stormpath.SDK.Impl.IdentityMap;
using Stormpath.SDK.Impl.Resource;
using Stormpath.SDK.Impl.Serialization;
using Stormpath.SDK.Logging;
using Stormpath.SDK.Serialization;
using Map = System.Collections.Generic.IDictionary<string, object>;

namespace Stormpath.SDK.Impl.DataStore
{
    internal sealed partial class DefaultDataStore : IInternalDataStore, IInternalAsyncDataStore, IInternalSyncDataStore, IDisposable
    {
        IRequestExecutor IInternalDataStore.RequestExecutor => this.requestExecutor;

        ICacheResolver IInternalDataStore.CacheResolver => this.cacheResolver;

        IJsonSerializer IInternalDataStore.Serializer => this.serializer.ExternalSerializer;

        string IInternalDataStore.BaseUrl => this.baseUrl;

        IClientApiKey IInternalDataStore.ApiKey => this.requestExecutor.ApiKey;

        internal DefaultDataStore(IRequestExecutor requestExecutor, string baseUrl, IJsonSerializer serializer, ILogger logger, ICacheProvider cacheProvider, TimeSpan identityMapExpiration)
        {
            if (requestExecutor == null)
            {
                throw new ArgumentNullException(nameof(requestExecutor));
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (cacheProvider == null)
            {
                throw new ArgumentNullException(nameof(cacheProvider), "Use NullCacheProvider if you wish to turn off caching.");
            }

            this.baseUrl = baseUrl;
            this.logger = logger;
            this.requestExecutor = requestExecutor;
            this.cacheProvider = cacheProvider;
            this.cacheResolver = new DefaultCacheResolver(cacheProvider, this.logger);

            this.serializer = new JsonSerializationProvider(serializer);
            this.identityMap = new MemoryCacheIdentityMap<ResourceData>(identityMapExpiration, this.logger);
            this.resourceFactory = new DefaultResourceFactory(this, this.identityMap);
            this.resourceConverter = new DefaultResourceConverter();

            this.uriQualifier = new UriQualifier(baseUrl);

            this.defaultAsyncFilters = this.BuildDefaultAsyncFilterChain();
            this.defaultSyncFilters = this.BuildDefaultSyncFilterChain();
        }

        T IDataStore.Instantiate<T>()
            => this.resourceFactory.Create<T>();

        T IInternalDataStore.InstantiateWithData<T>(Map properties)
            => this.resourceFactory.Create<T>(properties);

        T IInternalDataStore.InstantiateWithHref<T>(string href)
        {
            var properties = new Dictionary<string, object>()
            {
                ["href"] = href
            };
            return this.AsAsyncInterface.InstantiateWithData<T>(properties);
        }
    }
}