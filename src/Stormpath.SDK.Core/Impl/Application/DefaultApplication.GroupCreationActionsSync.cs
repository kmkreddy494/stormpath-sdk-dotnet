﻿// <copyright file="DefaultApplication.GroupCreationActionsSync.cs" company="Stormpath, Inc.">
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
using Stormpath.SDK.Group;
using Stormpath.SDK.Impl.Group;

namespace Stormpath.SDK.Impl.Application
{
    internal sealed partial class DefaultApplication
    {
        IGroup IGroupCreationActionsSync.CreateGroup(IGroup group)
            => GroupCreationActionsShared.CreateGroup(this.GetInternalSyncDataStore(), this.Groups.Href, group);

        IGroup IGroupCreationActionsSync.CreateGroup(IGroup group, Action<GroupCreationOptionsBuilder> creationOptionsAction)
            => GroupCreationActionsShared.CreateGroup(this.GetInternalSyncDataStore(), this.Groups.Href, group, creationOptionsAction);

        IGroup IGroupCreationActionsSync.CreateGroup(IGroup group, IGroupCreationOptions creationOptions)
            => GroupCreationActionsShared.CreateGroup(this.GetInternalSyncDataStore(), this.Groups.Href, group, creationOptions);

        IGroup IGroupCreationActionsSync.CreateGroup(string name, string description)
            => GroupCreationActionsShared.CreateGroup(this.GetInternalSyncDataStore(), this.Groups.Href, name, description);
    }
}
