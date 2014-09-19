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
using Hyperstore.Modeling.Commands;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A model entity.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ModelElement"/>
    /// <seealso cref="T:Hyperstore.Modeling.IModelEntity"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelEntity : ModelElement, IModelEntity
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected ModelEntity()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructeur. Si la clé est à null, une clé est générée automatiquement. Si le metadata est
        ///  Empty, un nouvel metadata est créé automatiquement.
        /// </summary>
        /// <exception cref="NotInTransactionException">
        ///  Thrown when a Not In Transaction error condition occurs.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///  Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="schemaName">
        ///  Name of the schema.
        /// </param>
        /// <param name="domainModel">
        ///  (Optional) Domain model auquel appartient l'élément.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected ModelEntity(string schemaName, IDomainModel domainModel = null)
        {
            Contract.RequiresNotEmpty(schemaName, "schemaName");

            if (Session.Current == null)
                throw new NotInTransactionException();

            if (domainModel == null)
            {
                domainModel = Session.Current.DefaultDomainModel;
                if (domainModel == null )
                    throw new ArgumentException("domainModel");
            }

            var metadata = EnsuresSchemaExists(domainModel, schemaName) as ISchemaEntity;

            Super(domainModel, metadata, (dm, melId, mid) => new AddEntityCommand(this));

            if (((IModelEntity)this).SchemaEntity == null)
                throw new Exception(ExceptionMessages.SchemaMismatch);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructeur. Si la clé est à null, une clé est générée automatiquement. Si le metadata est
        ///  Empty, un nouvel metadata est créé automatiquement.
        /// </summary>
        /// <exception>
        ///  <cref>TransactionMandatoryException</cref>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///  Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="domainModel">
        ///  (Optional) Domain model auquel appartient l'élément.
        /// </param>
        /// <param name="schemaEntity">
        ///  (Optional)
        /// </param>
        /// <param name="id">
        ///  (Optional)
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ModelEntity(IDomainModel domainModel = null, ISchemaEntity schemaEntity = null, Identity id = null)
        {
            if (Session.Current == null)
                throw new NotInTransactionException();

            if (domainModel == null)
            {
                domainModel = Session.Current.DefaultDomainModel;
                if (domainModel == null)
                    throw new ArgumentException("domainModel");
            }

            Super(domainModel, schemaEntity, (dm, melId, mid) => new AddEntityCommand(this), id);

            if (((IModelEntity)this).SchemaEntity == null)
                throw new Exception(ExceptionMessages.SchemaMismatch);
        }

        ISchemaEntity IModelEntity.SchemaEntity
        {
            get { return ((IModelElement) this).SchemaInfo as ISchemaEntity; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected override void Remove()
        {
            using (var session = EnsuresRunInSession())
            {
                var cmd = new RemoveEntityCommand(this);
                Session.Current.Execute(cmd);
                if (session != null)
                    session.AcceptChanges();
            }
        }
    }
}