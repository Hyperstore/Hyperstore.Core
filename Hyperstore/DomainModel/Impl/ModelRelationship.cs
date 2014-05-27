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
    ///  A model relationship.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ModelElement"/>
    /// <seealso cref="T:Hyperstore.Modeling.IModelRelationship"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelRelationship : ModelElement, IModelRelationship
    {
        private readonly ISchemaElement _endMetadata;
        private readonly ISchemaElement _startMetadata;
        private IModelElement _end;
        private Identity _endId;
        private IModelElement _start;
        private Identity _startId;
        private Identity _endSchemaId;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected ModelRelationship()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="startId">
        ///  The start identifier.
        /// </param>
        /// <param name="startSchema">
        ///  The start schema.
        /// </param>
        /// <param name="endId">
        ///  The end identifier.
        /// </param>
        /// <param name="endSchema">
        ///  The end schema.
        /// </param>
        /// <param name="schemaRelationship">
        ///  (Optional) the schema relationship.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ModelRelationship(IDomainModel domainModel, Identity startId, ISchemaElement startSchema, Identity endId, ISchemaElement endSchema, ISchemaRelationship schemaRelationship = null)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(startId, "startId");
            Contract.Requires(startSchema, "startSchema");
            Contract.Requires(endId, "endId");
            Contract.Requires(endSchema, "endSchema");

            _startId = startId;
            _endId = endId;
            _startMetadata = startSchema;
            _endMetadata = endSchema;
            _endSchemaId = endSchema.Id;

            // Appel du ctor hérité
            Super(domainModel, schemaRelationship, (dm, melId, mid) => new AddRelationshipCommand(dm, mid as ISchemaRelationship, _startId, _startMetadata, _endId, _endMetadata, melId));

            if (((IModelRelationship)this).SchemaRelationship == null)
                throw new Exception(ExceptionMessages.SchemaMismatch);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="schemaRelationship">
        ///  (Optional) the schema relationship.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ModelRelationship(IModelElement start, IModelElement end, ISchemaRelationship schemaRelationship = null)
        {
            Contract.Requires(start, "start");
            Contract.Requires(end, "end");

            _start = start;
            _end = end;
            _endSchemaId = end.SchemaInfo.Id;
            // Appel du ctor hérité
            Super(start.DomainModel, schemaRelationship, (dm, melId, mid) => new AddRelationshipCommand(mid as ISchemaRelationship, _start, _end, melId));

            if (((IModelRelationship)this).SchemaRelationship == null)
                throw new Exception(ExceptionMessages.SchemaMismatch);
        }

        ISchemaRelationship IModelRelationship.SchemaRelationship
        {
            get { return ((IModelElement)this).SchemaInfo as ISchemaRelationship; }
        }

        IModelElement IModelRelationship.Start
        {
            get { return this._start ?? (this._start = DomainModel.GetElement(this._startId, ((ISchemaRelationship) ((IModelElement) this).SchemaInfo).Start)); }
        }

        Identity IModelRelationship.EndId
        {
            get { return _endId; }
        }

        Identity IModelRelationship.EndSchemaId
        {
            get { return _endSchemaId; }
        }

        IModelElement IModelRelationship.End
        {
            get { return this._end ?? (this._end = Store.GetElement(this._endId, ((ISchemaRelationship) ((IModelElement) this).SchemaInfo).End)); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Called when [deserializing].
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="endSchemaId">
        ///  The end schema identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected override void OnDeserializing(ISchemaElement schemaElement, IDomainModel domainModel, string key, Identity start, Identity end, Identity endSchemaId)
        {
            DebugContract.Requires(start);
            DebugContract.Requires(end);

            base.OnDeserializing(schemaElement, domainModel, key, start, end, endSchemaId);

            _startId = start;
            _endId = end;
            _endSchemaId = endSchemaId; // Traitement spécial pour les noeuds terminaux faisant partie d'un autre domaine qui n'est peut-être pas chargé
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
                var cmd = new RemoveRelationshipCommand(this);
                Session.Current.Execute(cmd);
                if (session != null)
                    session.AcceptChanges();
            }
        }
    }
}