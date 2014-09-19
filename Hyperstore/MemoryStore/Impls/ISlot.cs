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