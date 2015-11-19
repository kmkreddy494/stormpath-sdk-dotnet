﻿// <copyright file="AbstractCacheProviderBuilder{T}.cs" company="Stormpath, Inc.">
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
using System.Linq;

namespace Stormpath.SDK.Cache
{
    public abstract class AbstractCacheProviderBuilder<T> : ICacheProviderBuilder
        where T : AbstractCacheProvider, new()
    {
        private readonly List<ICacheConfiguration> cacheConfigs
            = new List<ICacheConfiguration>();

        private TimeSpan? defaultTimeToLive;
        private TimeSpan? defaultTimeToIdle;

        ICacheProviderBuilder ICacheProviderBuilder.WithDefaultTimeToIdle(TimeSpan tti)
        {
            this.defaultTimeToIdle = tti;
            return this;
        }

        ICacheProviderBuilder ICacheProviderBuilder.WithDefaultTimeToLive(TimeSpan ttl)
        {
            this.defaultTimeToLive = ttl;
            return this;
        }

        ICacheProviderBuilder ICacheProviderBuilder.WithCache(ICacheConfigurationBuilder builder)
        {
            var cacheConfig = builder.Build();
            if (cacheConfig == null)
                throw new ApplicationException("The cache configuration is not valid.");

            this.cacheConfigs.Add(cacheConfig);
            return this;
        }

        protected virtual ICacheProvider OnBuilding(T provider)
        {
            return provider;
        }

        ICacheProvider ICacheProviderBuilder.Build()
        {
            var provider = new T();

            if (this.defaultTimeToLive.HasValue)
                provider.SetDefaultTimeToLive(this.defaultTimeToLive.Value);

            if (this.defaultTimeToIdle.HasValue)
                provider.SetDefaultTimeToIdle(this.defaultTimeToIdle.Value);

            if (this.cacheConfigs.Any())
                provider.SetCacheConfigurations(this.cacheConfigs);

            return this.OnBuilding(provider);
        }
    }
}