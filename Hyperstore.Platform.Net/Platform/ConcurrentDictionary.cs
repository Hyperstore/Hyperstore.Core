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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Platform.Net
{
    class ConcurrentDictionaryWrapper<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, IConcurrentDictionary<TKey, TValue>
    {
        public ConcurrentDictionaryWrapper()
            : base()
        {

        }

        bool IConcurrentDictionary<TKey, TValue>.TryAdd(TKey key, TValue value)
        {
            return base.TryAdd(key, value);
        }

        TValue IConcurrentDictionary<TKey, TValue>.GetOrAdd(TKey key, TValue value)
        {
            return base.GetOrAdd(key, value);
        }

        IEnumerable<TValue> IConcurrentDictionary<TKey, TValue>.Values
        {
            get { return base.Values; }
        }

        bool IConcurrentDictionary<TKey, TValue>.TryGetValue(TKey id, out TValue value)
        {
            return base.TryGetValue(id, out value);
        }

        TValue IConcurrentDictionary<TKey, TValue>.this[TKey index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value;
            }
        }
    }
}
