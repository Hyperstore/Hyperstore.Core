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
 
namespace Hyperstore.Modeling.MemoryStore
{
    /// <summary>
    ///     Unité de stockage d'une valeur dans le memory store
    /// </summary>
    internal interface ISlot
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Identifiant unique du slot.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        long Id { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Transaction ayant créée ou modifiée ce slot.
        /// </summary>
        /// <value>
        ///  The minimum value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        long XMin { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Transaction ayant supprimé ce slot (ou via un update)
        /// </summary>
        /// <value>
        ///  The maximum value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        long? XMax { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  N° de la commande dans la transaction qui a modifiée ce slot (add/update/delete)
        /// </summary>
        /// <value>
        ///  The minimum value.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        int CMin { get; set; }
    }
}