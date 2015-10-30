﻿// <copyright file="Application_tests.cs" company="Stormpath, Inc.">
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

using System.Linq;
using Shouldly;
using Stormpath.SDK.Application;
using Stormpath.SDK.Sync;
using Xunit;

namespace Stormpath.SDK.Tests.Integration.Sync
{
    [Collection(nameof(IntegrationTestCollection))]
    public class Application_tests
    {
        private readonly IntegrationTestFixture fixture;

        public Application_tests(IntegrationTestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Getting_tenant_applications(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = client.GetCurrentTenant();
            var applications = tenant.GetApplications().Synchronously().ToList();

            applications.Count.ShouldNotBe(0);
            applications
                .Any(app => app.Status == ApplicationStatus.Enabled)
                .ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Getting_application_tenant(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            // Verify data from IntegrationTestData
            var tenantHref = application.GetTenant().Href;
            tenantHref.ShouldBe(this.fixture.TenantHref);
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Creating_application_without_directory(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = client.GetCurrentTenant();

            var newApplicationName = $".NET IT {this.fixture.TestRunIdentifier} Application #2";
            var createdApplication = tenant.CreateApplication(newApplicationName, createDirectory: false);

            createdApplication.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedApplicationHrefs.Add(createdApplication.Href);
            createdApplication.Name.ShouldBe(newApplicationName);
            createdApplication.Status.ShouldBe(ApplicationStatus.Enabled);

            var defaultAccountStore = createdApplication.GetDefaultAccountStore();
            if (!string.IsNullOrEmpty(defaultAccountStore?.Href))
                this.fixture.CreatedDirectoryHrefs.Add(defaultAccountStore.Href);

            defaultAccountStore.ShouldBeNull(); // no auto-created directory = no default account store

            // Clean up
            var deleted = createdApplication.Delete();
            if (deleted)
                this.fixture.CreatedApplicationHrefs.Remove(createdApplication.Href);
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Creating_application_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = client.GetCurrentTenant();

            var newApp = client
                .Instantiate<IApplication>()
                .SetName($".NET IT {this.fixture.TestRunIdentifier} Application #3 - Sync");

            tenant.CreateApplication(newApp, opt => opt.ResponseOptions.Expand(x => x.GetCustomData));

            newApp.Href.ShouldNotBeNullOrEmpty();
            this.fixture.CreatedApplicationHrefs.Add(newApp.Href);

            // Clean up
            newApp.Delete();
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Updating_application(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = client.GetCurrentTenant();

            var application = tenant.GetApplications()
                .Synchronously()
                .Where(app => app.Name.StartsWith($".NET IT (disabled) {this.fixture.TestRunIdentifier}"))
                .Single();

            application.SetDescription("The Battle of Yavin - Victory!");
            var saveResult = application.Save();

            saveResult.Description.ShouldBe("The Battle of Yavin - Victory!");
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Saving_with_response_options(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = client.GetCurrentTenant();

            var application = tenant.GetApplications()
                .Synchronously()
                .Where(app => app.Name.StartsWith($".NET IT (disabled) {this.fixture.TestRunIdentifier}"))
                .Single();

            application.SetStatus(ApplicationStatus.Disabled);
            application.Save(response => response.Expand(x => x.GetAccounts));
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Searching_by_name(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = client.GetCurrentTenant();

            var application = tenant.GetApplications()
                .Synchronously()
                .Where(app => app.Name.StartsWith($".NET IT (primary) {this.fixture.TestRunIdentifier}"))
                .Single();

            application.Description.ShouldBe("The Battle of Endor");
            application.Status.ShouldBe(ApplicationStatus.Enabled);
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Searching_by_description(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = client.GetCurrentTenant();

            var applications = tenant.GetApplications()
                .Synchronously()
                .Where(app => app.Description == "The Battle Of Endor")
                .ToList();

            applications
                .Any(app => app.Name.StartsWith($".NET IT (primary) {this.fixture.TestRunIdentifier}"))
                .ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Searching_by_status(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var tenant = client.GetCurrentTenant();

            var applications = tenant.GetApplications()
                .Synchronously()
                .Where(app => app.Status == ApplicationStatus.Disabled)
                .ToList();

            applications
                .Any(app => app.Name.StartsWith($".NET IT (disabled) {this.fixture.TestRunIdentifier}"))
                .ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(IntegrationTestClients.GetClients), MemberType = typeof(IntegrationTestClients))]
        public void Reset_password_for_valid_account(TestClientProvider clientBuilder)
        {
            var client = clientBuilder.GetClient();
            var application = client.GetResource<IApplication>(this.fixture.PrimaryApplicationHref);

            var token = application.SendPasswordResetEmail("vader@galacticempire.co");

            var validTokenResponse = application.VerifyPasswordResetToken(token.GetValue());
            validTokenResponse.Email.ShouldBe("vader@galacticempire.co");

            var resetPasswordResponse = application.ResetPassword(token.GetValue(), "Ifindyourlackofsecuritydisturbing!1");
            resetPasswordResponse.Email.ShouldBe("vader@galacticempire.co");
        }
    }
}