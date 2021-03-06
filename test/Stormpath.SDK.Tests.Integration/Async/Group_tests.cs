﻿// <copyright file="Group_tests.cs" company="Stormpath, Inc.">
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

using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Stormpath.SDK.Application;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Group;
using Stormpath.SDK.Tests.Common.Integration;
using Xunit;

namespace Stormpath.SDK.Tests.Integration.Async
{
    [Collection(nameof(IntegrationTestCollection))]
    public class Group_tests
    {
        private readonly TestFixture fixture;

        public Group_tests(TestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_tenant_groups(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = await client.GetCurrentTenantAsync();
            var groups = await tenant.GetGroups().ToListAsync();

            groups.Count.ShouldBeGreaterThan(0);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_group_tenant(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var group = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryGroupHref);

            // Verify data from IntegrationTestData
            var tenantHref = (await group.GetTenantAsync()).Href;
            tenantHref.ShouldBe(this.fixture.TenantHref);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_application_groups(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var groups = await app.GetGroups().ToListAsync();

            groups.Count.ShouldBeGreaterThan(0);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_group_applications(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var group = await client.GetGroupAsync(this.fixture.PrimaryGroupHref);

            var apps = await group.GetApplications().ToListAsync();
            apps.Where(x => x.Href == this.fixture.PrimaryApplicationHref).Any().ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_directory_groups(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var directory = await client.GetResourceAsync<IDirectory>(this.fixture.PrimaryDirectoryHref);
            var groups = await directory.GetGroups().ToListAsync();

            groups.Count.ShouldBeGreaterThan(0);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_account_groups(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var luke = await app.GetAccounts().Where(x => x.Email.StartsWith("lskywalker")).SingleAsync();

            var groups = await luke.GetGroups().ToListAsync();

            groups.Count.ShouldBeGreaterThan(0);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_group_accounts(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var humans = await client.GetResourceAsync<IGroup>(this.fixture.PrimaryGroupHref);

            (await humans.GetAccounts().CountAsync()).ShouldBeGreaterThan(0);
            (await humans.GetAccountMemberships().CountAsync()).ShouldBeGreaterThan(0);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Modifying_group(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var directory = await client.GetResourceAsync<IDirectory>(this.fixture.PrimaryDirectoryHref);

            var droids = client
                .Instantiate<IGroup>()
                .SetName($"Droids (.NET ITs {this.fixture.TestRunIdentifier})")
                .SetDescription("Mechanical entities")
                .SetStatus(GroupStatus.Enabled);

            await directory.CreateGroupAsync(droids);
            this.fixture.CreatedGroupHrefs.Add(droids.Href);

            droids.SetStatus(GroupStatus.Disabled);
            var result = await droids.SaveAsync();

            result.Status.ShouldBe(GroupStatus.Disabled);

            // Clean up
            (await droids.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedGroupHrefs.Remove(droids.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Saving_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var directory = await client.GetResourceAsync<IDirectory>(this.fixture.PrimaryDirectoryHref);

            var newGroup = client
                .Instantiate<IGroup>()
                .SetName($"Another Group (.NET ITs {this.fixture.TestRunIdentifier})")
                .SetStatus(GroupStatus.Disabled);

            await directory.CreateGroupAsync(newGroup);
            this.fixture.CreatedGroupHrefs.Add(newGroup.Href);

            newGroup.SetDescription("foobar");
            await newGroup.SaveAsync(response => response.Expand(x => x.GetAccounts(0, 10)));

            // Clean up
            (await newGroup.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedGroupHrefs.Remove(newGroup.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Adding_account_to_group(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var humans = await client.GetResourceAsync<IGroup>(this.fixture.PrimaryGroupHref);

            var lando = await app.GetAccounts().Where(x => x.Email.StartsWith("lcalrissian")).SingleAsync();
            var membership = await humans.AddAccountAsync(lando);

            membership.ShouldNotBeNull();
            membership.Href.ShouldNotBeNullOrEmpty();

            (await lando.IsMemberOfGroupAsync(humans.Href)).ShouldBeTrue();

            (await humans.RemoveAccountAsync(lando)).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Getting_group_membership(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var humans = await client.GetResourceAsync<IGroup>(this.fixture.PrimaryGroupHref);

            var lando = await app.GetAccounts().Where(x => x.Email.StartsWith("lcalrissian")).SingleAsync();
            var membership = await humans.AddAccountAsync(lando);

            // Should also be seen in the master membership list
            var allMemberships = await humans.GetAccountMemberships().ToListAsync();
            allMemberships.ShouldContain(x => x.Href == membership.Href);

            (await membership.GetAccountAsync()).Href.ShouldBe(lando.Href);
            (await membership.GetGroupAsync()).Href.ShouldBe(humans.Href);

            (await membership.DeleteAsync()).ShouldBeTrue();

            (await lando.IsMemberOfGroupAsync(humans.Href)).ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Adding_account_to_group_by_group_href(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var leia = await app.GetAccounts().Where(x => x.Email.StartsWith("leia.organa")).SingleAsync();
            await leia.AddGroupAsync(this.fixture.PrimaryGroupHref);

            (await leia.IsMemberOfGroupAsync(this.fixture.PrimaryGroupHref)).ShouldBeTrue();

            (await leia.RemoveGroupAsync(this.fixture.PrimaryGroupHref)).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Adding_account_to_group_by_group_name(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var groupName = (await client.GetResourceAsync<IGroup>(this.fixture.PrimaryGroupHref)).Name;

            var han = await app.GetAccounts().Where(x => x.Email.StartsWith("han.solo")).SingleAsync();
            await han.AddGroupAsync(groupName);

            (await han.IsMemberOfGroupAsync(this.fixture.PrimaryGroupHref)).ShouldBeTrue();

            (await han.RemoveGroupAsync(groupName)).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Adding_account_to_group_by_account_href(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var humans = await client.GetResourceAsync<IGroup>(this.fixture.PrimaryGroupHref);

            var leia = await app.GetAccounts().Where(x => x.Email.StartsWith("leia.organa")).SingleAsync();
            await humans.AddAccountAsync(leia.Href);

            (await leia.IsMemberOfGroupAsync(this.fixture.PrimaryGroupHref)).ShouldBeTrue();

            (await humans.RemoveAccountAsync(leia.Href)).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Adding_account_to_group_by_account_email(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);
            var humans = await client.GetResourceAsync<IGroup>(this.fixture.PrimaryGroupHref);

            var han = await app.GetAccounts().Where(x => x.Email.StartsWith("han.solo")).SingleAsync();
            await humans.AddAccountAsync(han.Email);

            (await han.IsMemberOfGroupAsync(this.fixture.PrimaryGroupHref)).ShouldBeTrue();

            (await humans.RemoveAccountAsync(han.Email)).ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_group_in_application(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var instance = client.Instantiate<IGroup>();
            instance.SetName($".NET ITs New Test Group {this.fixture.TestRunIdentifier}");
            instance.SetDescription("A nu start");
            instance.SetStatus(GroupStatus.Disabled);

            var created = await app.CreateGroupAsync(instance);
            created.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedGroupHrefs.Add(created.Href);

            created.Name.ShouldBe($".NET ITs New Test Group {this.fixture.TestRunIdentifier}");
            created.Description.ShouldBe("A nu start");
            created.Status.ShouldBe(GroupStatus.Disabled);

            (await created.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedGroupHrefs.Remove(created.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_group_in_application_with_convenience_method(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var name = $".NET ITs New Convenient Test Group {this.fixture.TestRunIdentifier}";
            var created = await app.CreateGroupAsync(name, "I can has lazy");
            created.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedGroupHrefs.Add(created.Href);

            created.Name.ShouldBe(name);
            created.Description.ShouldBe("I can has lazy");
            created.Status.ShouldBe(GroupStatus.Enabled);

            (await created.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedGroupHrefs.Remove(created.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_group_in_directory(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var directory = await client.GetResourceAsync<IDirectory>(this.fixture.PrimaryDirectoryHref);

            var instance = client.Instantiate<IGroup>();
            var directoryName = $".NET ITs New Test Group #2 ({this.fixture.TestRunIdentifier} - {clientBuilder.Name})";
            instance.SetName(directoryName);
            instance.SetDescription("A nu start");
            instance.SetStatus(GroupStatus.Enabled);

            var created = await directory.CreateGroupAsync(instance);
            created.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedGroupHrefs.Add(created.Href);

            created.Name.ShouldBe(directoryName);
            created.Description.ShouldBe("A nu start");
            created.Status.ShouldBe(GroupStatus.Enabled);

            (await created.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedGroupHrefs.Remove(created.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_group_with_custom_data(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var instance = client.Instantiate<IGroup>();
            instance.SetName($".NET ITs Custom Data Group ({this.fixture.TestRunIdentifier} - {clientBuilder.Name})");
            instance.CustomData.Put("isNeat", true);
            instance.CustomData.Put("roleBasedSecurity", "pieceOfCake");

            var created = await app.CreateGroupAsync(instance);
            this.fixture.CreatedGroupHrefs.Add(created.Href);

            var customData = await created.GetCustomDataAsync();
            customData["isNeat"].ShouldBe(true);
            customData["roleBasedSecurity"].ShouldBe("pieceOfCake");

            (await created.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedGroupHrefs.Remove(created.Href);
        }

        [Theory]
        [MemberData(nameof(TestClients.GetClients), MemberType = typeof(TestClients))]
        public async Task Creating_group_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var app = await client.GetResourceAsync<IApplication>(this.fixture.PrimaryApplicationHref);

            var group = client
                .Instantiate<IGroup>()
                .SetName($".NET ITs Custom Data Group #2 ({this.fixture.TestRunIdentifier} - {clientBuilder.Name})");

            await app.CreateGroupAsync(group, opt => opt.ResponseOptions.Expand(x => x.GetCustomData()));

            group.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedGroupHrefs.Add(group.Href);

            (await group.DeleteAsync()).ShouldBeTrue();
            this.fixture.CreatedGroupHrefs.Remove(group.Href);
        }
    }
}
