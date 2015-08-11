﻿// <copyright file="AsyncQueryableBase.cs" company="Stormpath, Inc.">
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

using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;
using Stormpath.SDK.Linq;

namespace Stormpath.SDK.Impl.Linq
{
    internal class AsyncQueryableBase<T> : QueryableBase<T>
    {
        public AsyncQueryableBase(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }

        public AsyncQueryableBase(IQueryParser queryParser, IAsyncQueryExecutor asyncExecutor)
            : base(queryParser, asyncExecutor)
        {
            this.AsyncProvider = new AsyncQueryProviderWrapper(queryParser, asyncExecutor);
        }

        public IAsyncQueryProvider AsyncProvider { get; private set; }
    }
}