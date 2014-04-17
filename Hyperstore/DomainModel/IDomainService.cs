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
 
namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface impl�ment�e par les services associ�s � un domaine.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public interface IDomainService
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initialisation du service avec le domaine associ�. Cette m�thode est appel�e quand le service
        ///  est instanci� par le domaine.
        /// </summary>
        /// <param name="domainModel">
        ///  Domaine associ�.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        void SetDomain(IDomainModel domainModel);
    }
}