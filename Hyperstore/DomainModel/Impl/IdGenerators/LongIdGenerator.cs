//	Copyright © 2013 - 2014, Alain Metge. All rights reserved.
//
//		This file is part of Hyperstore (http://www.hyperstore.org)
//
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
 
#region Imports

using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Hyperstore.Modeling.HyperGraph;

#endregion

namespace Hyperstore.Modeling.Domain
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A long identifier generator.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.HyperGraph.IIdGenerator"/>
    ///-------------------------------------------------------------------------------------------------
    public class LongIdGenerator : IIdGenerator
    {
        private readonly object _sync = new object();
        private long _counter;
        private IDomainModel _domainModel;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="start">
        ///  (Optional) the start.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public LongIdGenerator(long start = 0)
        {
            _counter = start;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Nexts the value.
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <returns>
        ///  An Identity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        [DebuggerStepThrough]
        public Identity NextValue(ISchemaElement schemaElement)
        {
            return new Identity(_domainModel.Name, Interlocked.Increment(ref _counter)
                    .ToString(CultureInfo.InvariantCulture));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets the specified identifier.
        /// </summary>
        /// <param name="id">
        ///  The identifier to set.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Set(Identity id)
        {
            DebugContract.Requires(id);

            long value;
            if (long.TryParse(id.Key, out value))
            {
                if (value > _counter)
                {
                    // TODO ca doit pas être bon avec interlocked
                    lock (_sync)
                    {
                        if (value > _counter)
                            _counter = value;
                    }
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the current value.
        /// </summary>
        /// <value>
        ///  The current value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string CurrentValue
        {
            [DebuggerStepThrough]
            get { return _counter.ToString(CultureInfo.InvariantCulture); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets a domain.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel);
            _domainModel = domainModel;
        }
    }
}