//	Copyright � 2013 - 2014, Alain Metge. All rights reserved.
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