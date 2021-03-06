﻿// <copyright file="LinqHelper.cs" company="Stormpath, Inc.">
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
using System.Linq.Expressions;
using System.Reflection;

namespace Stormpath.SDK.Linq
{
    internal static class LinqHelper
    {
        public static Expression MethodCall(MethodInfo method, params Expression[] expressions) => Expression.Call(null, method, expressions);

        public static MethodInfo GetMethodInfo<T1>(Action<T1> func, T1 ignored) => func.GetMethodInfo();

        public static MethodInfo GetMethodInfo<T1, T2>(Action<T1, T2> func, T1 ignored1, T2 ignored2) => func.GetMethodInfo();

        public static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> func, T1 ignored) => func.GetMethodInfo();

        public static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> func, T1 ignored1, T2 ignored2) => func.GetMethodInfo();

        public static MethodInfo GetMethodInfo<T1, T2, T3, T4>(Func<T1, T2, T3, T4> func, T1 ignored1, T2 ignored2, T3 ignored3) => func.GetMethodInfo();

        public static MethodInfo GetMethodInfo<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5> func, T1 ignored1, T2 ignored2, T3 ignored3, T4 ignored4) => func.GetMethodInfo();
    }
}
