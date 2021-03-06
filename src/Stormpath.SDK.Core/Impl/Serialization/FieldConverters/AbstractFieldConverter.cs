﻿// <copyright file="AbstractFieldConverter.cs" company="Stormpath, Inc.">
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
using Stormpath.SDK.Impl.DataStore;

namespace Stormpath.SDK.Impl.Serialization.FieldConverters
{
    internal abstract class AbstractFieldConverter
    {
        public const Type AnyType = null;

        private readonly string name;
        private readonly Type[] appliesToTargetTypes;
        private readonly ResourceTypeLookup typeLookup;

        public AbstractFieldConverter(string converterName, Type appliesToTargetType = AnyType)
            : this(converterName, appliesToTargetTypes: appliesToTargetType)
        {
        }

        public AbstractFieldConverter(string converterName, params Type[] appliesToTargetTypes)
        {
            this.appliesToTargetTypes = appliesToTargetTypes;
            this.name = converterName;
            this.typeLookup = new ResourceTypeLookup();
        }

        public FieldConverterResult TryConvertField(KeyValuePair<string, object> token, Type targetType)
        {
            bool isSupported = false;
            if (this.appliesToTargetTypes.Contains(AnyType))
            {
                isSupported = true;
            }
            else
            {
                bool isSupportedTargetType = this.appliesToTargetTypes.Contains(targetType);
                bool isSupportedGenericTargetType = (targetType?.GetTypeInfo().IsGenericType ?? false) && this.appliesToTargetTypes.Contains(this.typeLookup.GetInnerCollectionInterface(targetType));

                isSupported = isSupportedTargetType || isSupportedGenericTargetType;
            }

            if (!isSupported)
            {
                return FieldConverterResult.Failed;
            }

            var result = this.ConvertImpl(token);
            return result;
        }

        protected abstract FieldConverterResult ConvertImpl(KeyValuePair<string, object> token);

        public override string ToString()
        {
            return this.name;
        }
    }
}
