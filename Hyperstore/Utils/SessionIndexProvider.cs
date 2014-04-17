// Copyright 2014 Zenasoft.  All rights reserved.
//
// This file is part of Hyperstore.
//
//    Hyperstore is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Hyperstore is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Hyperstore.  If not, see <http://www.gnu.org/licenses/>.
 
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