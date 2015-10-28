﻿// <copyright file="WriteCacheFilter.cs" company="Stormpath, Inc.">
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
using System.Threading;
using System.Threading.Tasks;
using Stormpath.SDK.Account;
using Stormpath.SDK.Auth;
using Stormpath.SDK.CustomData;
using Stormpath.SDK.Impl.Account;
using Stormpath.SDK.Impl.Cache;
using Stormpath.SDK.Impl.Extensions;
using Stormpath.SDK.Impl.Resource;
using Stormpath.SDK.Provider;
using Stormpath.SDK.Shared;

namespace Stormpath.SDK.Impl.DataStore.Filters
{
    internal sealed class WriteCacheFilter : AbstractCacheFilter, IAsynchronousFilter, ISynchronousFilter
    {
        private readonly IResourceFactory resourceFactory;
        private readonly ResourceTypes resourceTypes;

        public WriteCacheFilter(ICacheResolver cacheResolver, IResourceFactory resourceFactory)
            : base(cacheResolver)
        {
            if (resourceFactory == null)
                throw new ArgumentNullException(nameof(resourceFactory));

            this.resourceFactory = resourceFactory;
            this.resourceTypes = new ResourceTypes();
        }

        public override async Task<IResourceDataResult> FilterAsync(IResourceDataRequest request, IAsynchronousFilterChain chain, ILogger logger, CancellationToken cancellationToken)
        {
            bool isDelete = request.Action == ResourceAction.Delete;
            bool isCustomDataPropertyRequest = request.Uri.ResourcePath.ToString().Contains("/customData/");

            if (isCustomDataPropertyRequest && isDelete)
            {
                await this.UncacheCustomDataPropertyAsync(request.Uri.ResourcePath, cancellationToken).ConfigureAwait(false);
            }
            else if (isDelete)
            {
                var cacheKey = this.GetCacheKey(request);
                await this.UncacheAsync(request.ResourceType, cacheKey, cancellationToken).ConfigureAwait(false);
            }

            var result = await chain.ExecuteAsync(request, logger, cancellationToken).ConfigureAwait(false);

            bool isEmailVerificationResponse = result.Type == typeof(IEmailVerificationToken);
            if (isEmailVerificationResponse)
            {
                await this.UncacheAccountOnEmailVerificationAsync(result, cancellationToken).ConfigureAwait(false);
            }

            bool possibleCustomDataUpdate = (request.Action == ResourceAction.Create || request.Action == ResourceAction.Update) &&
                AbstractExtendableInstanceResource.IsExtendable(request.ResourceType);
            if (possibleCustomDataUpdate)
                await this.CacheNestedCustomDataUpdatesAsync(request, result, cancellationToken).ConfigureAwait(false);

            if (IsCacheable(request, result))
                await this.CacheAsync(result.Type, result.Body, cancellationToken).ConfigureAwait(false);

            return result;
        }

        public override IResourceDataResult Filter(IResourceDataRequest request, ISynchronousFilterChain chain, ILogger logger)
        {
            bool isDelete = request.Action == ResourceAction.Delete;
            bool isCustomDataPropertyRequest = request.Uri.ResourcePath.ToString().Contains("/customData/");

            if (isCustomDataPropertyRequest && isDelete)
            {
                this.UncacheCustomDataProperty(request.Uri.ResourcePath);
            }
            else if (isDelete)
            {
                var cacheKey = this.GetCacheKey(request);
                this.Uncache(request.ResourceType, cacheKey);
            }

            var result = chain.Filter(request, logger);

            bool isEmailVerificationResponse = result.Type == typeof(IEmailVerificationToken);
            if (isEmailVerificationResponse)
            {
                this.UncacheAccountOnEmailVerification(result);
            }

            bool possibleCustomDataUpdate = (request.Action == ResourceAction.Create || request.Action == ResourceAction.Update) &&
                AbstractExtendableInstanceResource.IsExtendable(request.ResourceType);
            if (possibleCustomDataUpdate)
                this.CacheNestedCustomDataUpdates(request, result);

            if (IsCacheable(request, result))
                this.Cache(result.Type, result.Body);

            return result;
        }

        private async Task CacheAsync(Type resourceType, IDictionary<string, object> data, CancellationToken cancellationToken)
        {
            string href = data[AbstractResource.HrefPropertyName].ToString();

            var cacheData = new Dictionary<string, object>();

            bool isCustomData = resourceType == typeof(ICustomData);
            if (isCustomData)
            {
                var cache = await this.GetCacheAsync(resourceType, cancellationToken).ConfigureAwait(false);
                await cache.PutAsync(this.GetCacheKey(href), data, cancellationToken).ConfigureAwait(false);
                return; // simple! return early
            }

            foreach (var item in data)
            {
                string key = item.Key;
                object value = item.Value;

                // TODO DefaultModelMap edge case
                // TODO ApiEncryptionMetadata edge case
                var asNestedResource = value as ExpandedProperty;
                var asNestedArray = value as IEnumerable<IDictionary<string, object>>;

                if (asNestedResource != null && IsResource(asNestedResource.Data))
                {
                    var nestedType = this.resourceTypes.GetInterface(item.Key);
                    if (nestedType == null)
                        throw new ApplicationException($"Cannot cache nested item. Item type for '{item.Key}' unknown.");

                    await this.CacheAsync(nestedType, asNestedResource.Data, cancellationToken).ConfigureAwait(false);
                    value = ToCanonicalReference(key, asNestedResource.Data);
                }
                else if (asNestedArray != null)
                {
                    // This is a CollectionResponsePage<T>.Items property. Find the type of objects to expect
                    var nestedType = this.resourceTypes.GetInnerCollectionInterface(resourceType);
                    if (nestedType == null)
                        throw new ApplicationException($"Can not cache array '{key}'. Item type for '{resourceType.Name}' unknown.");

                    // Recursively cache nested resources and create a new collection that only has references
                    var canonicalList = new List<object>();
                    foreach (var element in asNestedArray)
                    {
                        object canonicalElement = element;
                        var resourceElement = canonicalElement as IDictionary<string, object>;
                        if (resourceElement != null)
                        {
                            if (IsResource(resourceElement))
                            {
                                await this.CacheAsync(nestedType, resourceElement, cancellationToken).ConfigureAwait(false);
                                canonicalElement = ToCanonicalReference(key, resourceElement);
                            }
                        }

                        canonicalList.Add(canonicalElement);
                    }
                }

                bool isSensitive = DefaultAccount.PasswordPropertyName.Equals(key);
                if (!isSensitive)
                    cacheData.Add(key, value);
            }

            if (!ResourceTypes.IsCollectionResponse(resourceType))
            {
                var cache = await this.GetCacheAsync(resourceType, cancellationToken).ConfigureAwait(false);
                var cacheKey = this.GetCacheKey(href);
                await cache.PutAsync(cacheKey, cacheData, cancellationToken).ConfigureAwait(false);
            }
        }

        private void Cache(Type resourceType, IDictionary<string, object> data)
        {
            string href = data[AbstractResource.HrefPropertyName].ToString();

            var cacheData = new Dictionary<string, object>();

            bool isCustomData = resourceType == typeof(ICustomData);
            if (isCustomData)
            {
                var cache = this.GetCache(resourceType);
                cache.Put(this.GetCacheKey(href), data);
                return; // simple! return early
            }

            foreach (var item in data)
            {
                string key = item.Key;
                object value = item.Value;

                // TODO DefaultModelMap edge case
                // TODO ApiEncryptionMetadata edge case
                var asNestedResource = value as ExpandedProperty;
                var asNestedArray = value as IEnumerable<IDictionary<string, object>>;

                if (asNestedResource != null && IsResource(asNestedResource.Data))
                {
                    var nestedType = this.resourceTypes.GetInterface(item.Key);
                    if (nestedType == null)
                        throw new ApplicationException($"Cannot cache nested item. Item type for '{item.Key}' unknown.");

                    this.Cache(nestedType, asNestedResource.Data);
                    value = ToCanonicalReference(key, asNestedResource.Data);
                }
                else if (asNestedArray != null)
                {
                    // This is a CollectionResponsePage<T>.Items property. Find the type of objects to expect
                    var nestedType = this.resourceTypes.GetInnerCollectionInterface(resourceType);
                    if (nestedType == null)
                        throw new ApplicationException($"Can not cache array '{key}'. Item type for '{resourceType.Name}' unknown.");

                    // Recursively cache nested resources and create a new collection that only has references
                    var canonicalList = new List<object>();
                    foreach (var element in asNestedArray)
                    {
                        object canonicalElement = element;
                        var resourceElement = canonicalElement as IDictionary<string, object>;
                        if (resourceElement != null)
                        {
                            if (IsResource(resourceElement))
                            {
                                this.Cache(nestedType, resourceElement);
                                canonicalElement = ToCanonicalReference(key, resourceElement);
                            }
                        }

                        canonicalList.Add(canonicalElement);
                    }
                }

                bool isSensitive = DefaultAccount.PasswordPropertyName.Equals(key);
                if (!isSensitive)
                    cacheData.Add(key, value);
            }

            if (!ResourceTypes.IsCollectionResponse(resourceType))
            {
                var cache = this.GetCache(resourceType);
                var cacheKey = this.GetCacheKey(href);
                cache.Put(cacheKey, cacheData);
            }
        }

        private async Task CacheNestedCustomDataUpdatesAsync(IResourceDataRequest request, IResourceDataResult result, CancellationToken cancellationToken)
        {
            object customDataObj = null;
            IDictionary<string, object> customData = null;

            if (!request.Properties.TryGetValue(AbstractExtendableInstanceResource.CustomDataPropertyName, out customDataObj))
                return;

            customData = customDataObj as IDictionary<string, object>;
            if (customData.IsNullOrEmpty())
                return;

            bool creating = request.Action == ResourceAction.Create;

            var parentHref = request.Uri.ResourcePath.ToString();
            if (creating && !result.Body.TryGetValueAsString(AbstractResource.HrefPropertyName, out parentHref))
                return;

            var customDataHref = parentHref + "/customData";

            var dataToCache = await this.GetCachedValueAsync(typeof(ICustomData), customDataHref, cancellationToken).ConfigureAwait(false);
            if (!creating && dataToCache == null)
                return;

            if (dataToCache.IsNullOrEmpty())
                dataToCache = new Dictionary<string, object>();

            foreach (var updatedItem in customData)
            {
                dataToCache[updatedItem.Key] = updatedItem.Value;
            }

            // Ensure the href property exists
            dataToCache[AbstractResource.HrefPropertyName] = customDataHref;

            await this.CacheAsync(typeof(ICustomData), dataToCache, cancellationToken).ConfigureAwait(false);
        }

        private void CacheNestedCustomDataUpdates(IResourceDataRequest request, IResourceDataResult result)
        {
            object customDataObj = null;
            IDictionary<string, object> customData = null;

            if (!request.Properties.TryGetValue(AbstractExtendableInstanceResource.CustomDataPropertyName, out customDataObj))
                return;

            customData = customDataObj as IDictionary<string, object>;
            if (customData.IsNullOrEmpty())
                return;

            bool creating = request.Action == ResourceAction.Create;

            var parentHref = request.Uri.ResourcePath.ToString();
            if (creating && !result.Body.TryGetValueAsString(AbstractResource.HrefPropertyName, out parentHref))
                return;

            var customDataHref = parentHref + "/customData";

            var dataToCache = this.GetCachedValue(typeof(ICustomData), customDataHref);
            if (!creating && dataToCache == null)
                return;

            if (dataToCache.IsNullOrEmpty())
                dataToCache = new Dictionary<string, object>();

            foreach (var updatedItem in customData)
            {
                dataToCache[updatedItem.Key] = updatedItem.Value;
            }

            // Ensure the href property exists
            dataToCache[AbstractResource.HrefPropertyName] = customDataHref;

            this.Cache(typeof(ICustomData), dataToCache);
        }

        private async Task UncacheAsync(Type resourceType, string cacheKey, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(cacheKey))
                throw new ArgumentNullException(nameof(cacheKey));
            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType));

            var cache = await this.GetCacheAsync(resourceType, cancellationToken).ConfigureAwait(false);
            await cache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        }

        private void Uncache(Type resourceType, string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
                throw new ArgumentNullException(nameof(cacheKey));
            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType));

            var cache = this.GetCache(resourceType);
            cache.Remove(cacheKey);
        }

        private async Task UncacheCustomDataPropertyAsync(Uri resourceUri, CancellationToken cancellationToken)
        {
            var href = resourceUri.ToString();
            var propertyName = href.Substring(href.LastIndexOf('/') + 1);
            href = href.Substring(0, href.LastIndexOf('/'));

            if (string.IsNullOrEmpty(propertyName) ||
                string.IsNullOrEmpty(href))
                throw new ApplicationException("Could not update cache for removed custom data entry.");

            var cache = await this.GetCacheAsync(typeof(ICustomData), cancellationToken).ConfigureAwait(false);
            var cacheKey = this.GetCacheKey(href);

            var existingData = await cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
            if (existingData.IsNullOrEmpty())
                return;

            existingData.Remove(propertyName);
            await cache.PutAsync(cacheKey, existingData, cancellationToken).ConfigureAwait(false);
        }

        private void UncacheCustomDataProperty(Uri resourceUri)
        {
            var href = resourceUri.ToString();
            var propertyName = href.Substring(href.LastIndexOf('/') + 1);
            href = href.Substring(0, href.LastIndexOf('/'));

            if (string.IsNullOrEmpty(propertyName) ||
                string.IsNullOrEmpty(href))
                throw new ApplicationException("Could not update cache for removed custom data entry.");

            var cache = this.GetCache(typeof(ICustomData));
            var cacheKey = this.GetCacheKey(href);

            var existingData = cache.Get(cacheKey);
            if (existingData.IsNullOrEmpty())
                return;

            existingData.Remove(propertyName);
            cache.Put(cacheKey, existingData);
        }

        private async Task UncacheAccountOnEmailVerificationAsync(IResourceDataResult result, CancellationToken cancellationToken)
        {
            object accountHrefRaw = null;
            string accountHref = null;
            if (!result.Body.TryGetValue(AbstractResource.HrefPropertyName, out accountHrefRaw))
                return;

            accountHref = accountHrefRaw.ToString();
            if (string.IsNullOrEmpty(accountHref))
                return;

            var cache = await this.GetCacheAsync(typeof(IAccount), cancellationToken).ConfigureAwait(false);
            await cache.RemoveAsync(this.GetCacheKey(accountHref), cancellationToken).ConfigureAwait(false);
        }

        private void UncacheAccountOnEmailVerification(IResourceDataResult result)
        {
            object accountHrefRaw = null;
            string accountHref = null;
            if (!result.Body.TryGetValue(AbstractResource.HrefPropertyName, out accountHrefRaw))
                return;

            accountHref = accountHrefRaw.ToString();
            if (string.IsNullOrEmpty(accountHref))
                return;

            var cache = this.GetCache(typeof(IAccount));
            cache.Remove(this.GetCacheKey(accountHref));
        }

        private static bool IsCacheable(IResourceDataRequest request, IResourceDataResult result)
        {
            bool hasData = !result.Body.IsNullOrEmpty();

            return

                // Must be a resource
                IsResource(result?.Body) &&

                // Don't cache password reset or email verificaiton requests
                result.Type != typeof(IPasswordResetToken) &&
                result.Type != typeof(IEmailVerificationToken) &&
                result.Type != typeof(IEmailVerificationRequest) &&

                // Don't cache login attempts
                result.Type != typeof(IAuthenticationResult) &&

                // ProviderAccountResults look like IAccounts but should not be cached either
                result.Type != typeof(IProviderAccountResult);
        }

        private static bool IsResource(IDictionary<string, object> data)
        {
            if (data == null)
                return false;

            bool hasItems = data.Count > 1;
            bool hasHref = data.ContainsKey(AbstractResource.HrefPropertyName);

            return hasHref && hasItems;
        }

        private static object ToCanonicalReference(string propertyName, IDictionary<string, object> resourceData)
        {
            if (IsResource(resourceData))
                return new LinkProperty(resourceData[AbstractResource.HrefPropertyName].ToString());

            throw new ApplicationException($"Could not convert embedded resource '{propertyName}' to a canonical reference.");
        }
    }
}
