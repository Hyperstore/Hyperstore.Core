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
using System.Text;

namespace Hyperstore.Modeling
{

    class CalculatedProperty : IDisposable
    {
        public Action Handler { get; set; }

        public string PropertyName { get; private set; }

        private readonly HashSet<CalculatedProperty> _targets = new HashSet<CalculatedProperty>();

        public CalculatedProperty(string propertyName)
        {
            DebugContract.RequiresNotEmpty(propertyName);
            PropertyName = propertyName;
        }

        internal T GetResult<T>(Func<T> calculation)
        {
            var result = calculation();
            return result;
        }

        internal void Notify(HashSet<CalculatedProperty> set)
        {
            if (Handler != null)
                Handler();

            if( _targets.Count > 0)
                NotifyTargets(set);
        }

        void IDisposable.Dispose()
        {
            Handler = null;
            _targets.Clear();
        }

        internal void AddTarget(CalculatedProperty calculatedProperty)
        {
            _targets.Add(calculatedProperty);
        }

        internal void NotifyTargets(HashSet<CalculatedProperty> circularGuards = null)
        {
            var set = circularGuards ?? new HashSet<CalculatedProperty>();
            foreach(var target in _targets)
            {
                if (set.Add(target))
                {
                    target.Notify(set);
                }
            }
        }

    }
}
