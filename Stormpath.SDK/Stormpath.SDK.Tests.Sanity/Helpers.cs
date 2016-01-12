﻿// <copyright file="Helpers.cs" company="Stormpath, Inc.">
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Stormpath.SDK.Tests.Sanity
{
    public class Helpers
    {
        public static readonly string NL = Environment.NewLine;

        public static string GetQualifiedMethodName(MethodInfo m)
            => $"{m.DeclaringType.Name}.{m.Name}";

        public static string PrettyMethodOutput(IEnumerable<MethodInfo> methods)
        {
            if (!methods.Any())
            {
                return null;
            }

            var prettyMethods = methods.Select(m =>
            {
                return $"{m.Name} (in {m.DeclaringType.Name})";
            });

            return string.Join(NL, prettyMethods);
        }

        public static string PrettyPrintMethod(string qualifiedMethodName, IEnumerable<ParameterInfo> args)
        {
            return $"{qualifiedMethodName}({string.Join(", ", args.Select(p => p.ParameterType.Name))})";
        }

        /// <summary>
        /// Determines whether a particular type is compiler-generated.
        /// <para>Courtesy of Cameron MacFarland at http://stackoverflow.com/a/11839713/3191599</para>
        /// </summary>
        /// <param name="t">The type.</param>
        /// <returns><see langword="true"/> if this type is generated by the compiler; <see langword="false"/> otherwise.</returns>
        public static bool IsCompilerGenerated(Type t)
        {
            if (t == null)
            {
                return false;
            }

            return t.IsDefined(typeof(CompilerGeneratedAttribute), false)
                || IsCompilerGenerated(t.DeclaringType);
        }

        /// <summary>
        /// Tries to get the VB.NET Integration Test assembly.
        /// </summary>
        /// <remarks>We are intentionally not adding this as a simple project reference, because
        /// Mono is currently unable to build the VB project. This gets around that limitation,
        /// although it means that the associated test(s) can only run under Windows.</remarks>
        /// <returns>The VB.NET IT assembly, or <see langword="null"/>.</returns>
        public static Assembly GetVisualBasicIntegrationTestAssembly()
        {
            Assembly foundAssembly = null;
            try
            {
                var s = System.IO.Path.DirectorySeparatorChar;
                var relativePath = $"..{s}..{s}..{s}Stormpath.SDK.Tests.Integration.VB{s}bin{s}Debug{s}Stormpath.SDK.Tests.Integration.VB.dll";
                var absolutePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, relativePath));

                foundAssembly = Assembly.LoadFile(absolutePath);
            }
            catch (System.IO.FileNotFoundException)
            {
            }

            return foundAssembly;
        }
    }
}
