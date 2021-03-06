﻿// <copyright file="RandomPassword_tests.cs" company="Stormpath, Inc.">
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
using FluentAssertions;
using Xunit;

namespace Stormpath.SDK.Tests.Common.RandomData
{
    public class RandomPassword_tests
    {
        [Fact]
        public void Meets_minimum_length()
        {
            string password = new RandomPassword(12);

            password.Length.Should().BeGreaterOrEqualTo(12);
        }

        [Fact]
        public void Meets_minimum_requirements()
        {
            string password = new RandomPassword(3);

            // Requirement: 1 lowercase, 1 uppercase, 1 number, 1 symbol, no whitespace
            password.Any(x => char.IsUpper(x)).Should().BeTrue();
            password.Any(x => char.IsLower(x)).Should().BeTrue();
            password.Any(x => "!@#$%&|?".Contains(x)).Should().BeTrue();
            password.Any(x => char.IsNumber(x)).Should().BeTrue();
            password.Any(x => char.IsWhiteSpace(x)).Should().BeFalse();
        }
    }
}
