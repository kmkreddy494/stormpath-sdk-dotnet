﻿// <copyright file="DefaultAccount.cs" company="Stormpath, Inc.">
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

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.SDK.Account;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Group;
using Stormpath.SDK.Impl.DataStore;
using Stormpath.SDK.Impl.Resource;
using Stormpath.SDK.Resource;
using Stormpath.SDK.Tenant;

namespace Stormpath.SDK.Impl.Account
{
    internal sealed class DefaultAccount : AbstractExtendableInstanceResource, IAccount
    {
        private static readonly string AccessTokensPropertyName = "accessTokens";
        private static readonly string ApiKeysPropertyName = "apiKeys";
        private static readonly string ApplicationsPropertyName = "applications";
        private static readonly string DirectoryPropertyName = "directory";
        private static readonly string EmailPropertyName = "email";
        private static readonly string EmailVerificationTokenPropertyName = "emailVerificationToken";
        private static readonly string FullNamePropertyName = "fullName";
        private static readonly string GivenNamePropertyName = "givenName";
        private static readonly string GroupMembershipsPropertyName = "groupMemberships";
        private static readonly string GroupsPropertyName = "groups";
        private static readonly string MiddleNamePropertyName = "middleName";
        private static readonly string PasswordPropertyName = "password";
        private static readonly string ProviderDataPropertyName = "providerData";
        private static readonly string RefreshTokensPropertyName = "refreshTokens";
        private static readonly string StatusPropertyName = "status";
        private static readonly string SurnamePropertyName = "surname";
        private static readonly string TenantPropertyName = "tenant";
        private static readonly string UsernamePropertyName = "username";

        public DefaultAccount(IInternalDataStore dataStore)
            : base(dataStore)
        {
        }

        public DefaultAccount(IInternalDataStore dataStore, Hashtable properties)
            : base(dataStore, properties)
        {
        }

        internal LinkProperty AccessTokens => GetLinkProperty(AccessTokensPropertyName);

        internal LinkProperty ApiKeys => GetLinkProperty(ApiKeysPropertyName);

        internal LinkProperty Applications => GetLinkProperty(ApplicationsPropertyName);

        internal LinkProperty Directory => GetLinkProperty(DirectoryPropertyName);

        string IAccount.Email => GetProperty<string>(EmailPropertyName);

        internal LinkProperty EmailVerificationToken => GetLinkProperty(EmailVerificationTokenPropertyName);

        string IAccount.FullName => GetProperty<string>(FullNamePropertyName);

        string IAccount.GivenName => GetProperty<string>(GivenNamePropertyName);

        internal LinkProperty GroupMemberships => GetLinkProperty(GroupMembershipsPropertyName);

        internal LinkProperty Groups => GetLinkProperty(GroupsPropertyName);

        string IAccount.MiddleName => GetProperty<string>(MiddleNamePropertyName);

        internal LinkProperty ProviderData => GetLinkProperty(ProviderDataPropertyName);

        internal LinkProperty RefreshTokens => GetLinkProperty(RefreshTokensPropertyName);

        AccountStatus IAccount.Status => GetProperty<AccountStatus>(StatusPropertyName);

        string IAccount.Surname => GetProperty<string>(SurnamePropertyName);

        internal LinkProperty Tenant => GetLinkProperty(TenantPropertyName);

        string IAccount.Username => GetProperty<string>(UsernamePropertyName);

        IAccount IAccount.SetEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            SetProperty(EmailPropertyName, email);
            return this;
        }

        IAccount IAccount.SetGivenName(string givenName)
        {
            if (string.IsNullOrEmpty(givenName))
                throw new ArgumentNullException(nameof(givenName));

            SetProperty(GivenNamePropertyName, givenName);
            return this;
        }

        IAccount IAccount.SetMiddleName(string middleName)
        {
            SetProperty(MiddleNamePropertyName, middleName);
            return this;
        }

        IAccount IAccount.SetPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            SetProperty(PasswordPropertyName, password);
            return this;
        }

        IAccount IAccount.SetStatus(AccountStatus status)
        {
            SetProperty(StatusPropertyName, status);
            return this;
        }

        IAccount IAccount.SetSurname(string surname)
        {
            if (string.IsNullOrEmpty(surname))
                throw new ArgumentNullException(nameof(surname));

            SetProperty(SurnamePropertyName, surname);
            return this;
        }

        IAccount IAccount.SetUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            SetProperty(username, username);
            return this;
        }

        Task<IDirectory> IAccount.GetDirectoryAsync()
        {
            throw new NotImplementedException();
        }

        Task<ICollectionResourceQueryable<IGroup>> IAccount.GetGroupsAsync()
        {
            throw new NotImplementedException();
        }

        Task<ITenant> IAccount.GetTenantAsync()
        {
            throw new NotImplementedException();
        }

        Task<bool> IDeletable.DeleteAsync(CancellationToken cancellationToken)
        {
            return GetInternalDataStore().DeleteAsync(this, cancellationToken);
        }

        Task<IAccount> ISaveable<IAccount>.SaveAsync(CancellationToken cancellationToken)
        {
            return GetInternalDataStore().SaveAsync<IAccount>(this, cancellationToken);
        }
    }
}