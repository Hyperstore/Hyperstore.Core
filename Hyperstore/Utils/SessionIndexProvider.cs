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

using System;
using System.Linq;

#endregion

namespace Hyperstore.Modeling.Utils
{
    internal class SessionIndexProvider
    {
        private const int size = 32;
        private readonly uint[] _values;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public SessionIndexProvider()
        {
            _values = new uint[1024/size];
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Recherche de la 1ère position libre (= 0)
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <returns>
        ///  The first free value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ushort GetFirstFreeValue()
        {
            lock (_values)
            {
                for (var i = 0; i < _values.Length; i++)
                {
                    var b = _values[i];
                    if (b == 0xFFFFFFFF)
                        continue;

                    var mask = 0x80000000;
                    for (var x = 0; x < size; x++)
                    {
                        if ((b & mask) == 0)
                        {
                            // On bloque la position
                            _values[i] |= mask;
                            // Calcul du numéro de l'index correspondant au bit 
                            var r = (i*size) + x + 1;
                            return (ushort) r;
                        }
                        mask >>= 1;
                    }
                }
            }

            throw new Exception(ExceptionMessages.FatalErrorTooManySessions);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Libère une position.
        /// </summary>
        /// <param name="v">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void ReleaseValue(ushort v)
        {
            lock (_values)
            {
                var pos = (v - 1)/size;
                _values[pos] &= (uint) ~(0x80000000 >> (v - (pos*size) - 1));
            }
        }

        internal void Set(int v) // pour les test
        {
            lock (_values)
            {
                var pos = (v - 1)/size;
                _values[pos] |= (uint) (0x80000000 >> (v - (pos*size) - 1));
            }
        }
    }
}