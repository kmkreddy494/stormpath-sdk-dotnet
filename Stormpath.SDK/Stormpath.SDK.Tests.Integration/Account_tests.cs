﻿// <copyright file="Account_tests.cs" company="Stormpath, Inc.">
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

using System.Threading.Tasks;
using Shouldly;
using Stormpath.SDK.Account;
using Stormpath.SDK.Application;
using Stormpath.SDK.Tests.Integration.Helpers;
using Xunit;

namespace Stormpath.SDK.Tests.Integration
{
    [Collection("Live tenant tests")]
    public class Account_tests
    {
        private readonly IntegrationTestFixture fixture;

        public Account_tests(IntegrationTestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public async Task Getting_tenant_accounts(TestClientBuilder clientBuilder)
        {
            var client = clientBuilder.Build();
            var tenant = await client.GetCurrentTenantAsync();

            var accounts = await tenant.GetAccounts().ToListAsync();

            accounts.Count.ShouldNotBe(0);
        }

        [Theory]
        public void Getting_tenant_accounts_with_search()
        {

        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public async Task Creating_and_deleting_account(TestClientBuilder clientBuilder)
        {
            var client = clientBuilder.Build();
            var application = await client.GetResourceAsync<IApplication>(fixture.ApplicationHref);

            var randomEmail = new RandomEmail(at: "integrationtestingrocks.co");
            var account = await application.CreateAccountAsync("Luke", "Skywalker", randomEmail, new RandomPassword(12));

            account.Href.ShouldNotBeNullOrEmpty();
            account.FullName.ShouldBe("Luke Skywalker");
            account.Email.ShouldBe(randomEmail);
            account.Username.ShouldBe(randomEmail);
            account.Status.ShouldBe(AccountStatus.Enabled);

            var deleteResult = await account.DeleteAsync();
            deleteResult.ShouldBe(true);
        }
    }
}
