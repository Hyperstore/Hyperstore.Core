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
            ThrowIfDisposed();
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