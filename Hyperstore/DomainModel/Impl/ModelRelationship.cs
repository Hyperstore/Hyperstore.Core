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
    ///  A model relationship.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.ModelElement"/>
    /// <seealso cref="T:Hyperstore.Modeling.IModelRelationship"/>
    ///-------------------------------------------------------------------------------------------------
    public class ModelRelationship : ModelElement, IModelRelationship
    {
        private readonly ISchemaElement _endMetadata;
        private readonly ISchemaElement _startMetadata;
      //  private IModelElement _end;
        private Identity _endId;
      //  private IModelElement _start;
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

            _startId = start.Id;
            _endId = end.Id;
            _endSchemaId = end.SchemaInfo.Id;
            // Appel du ctor hérité
            Super(start.DomainModel, schemaRelationship, (dm, melId, mid) => new AddRelationshipCommand(mid as ISchemaRelationship, start, end, melId));

            if (((IModelRelationship)this).SchemaRelationship == null)
                throw new Exception(ExceptionMessages.SchemaMismatch);
        }

        ISchemaRelationship IModelRelationship.SchemaRelationship
        {
            get { return ((IModelElement)this).SchemaInfo as ISchemaRelationship; }
        }

        IModelElement IModelRelationship.Start
        {
            get { return  (DomainModel.GetElement(this._startId, ((ISchemaRelationship) ((IModelElement) this).SchemaInfo).Start)); }
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
            get { return  (Store.GetElement(this._endId, ((ISchemaRelationship) ((IModelElement) this).SchemaInfo).End)); }
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("{0} : {1} ({2}->{3})", GetType().Name, ((IModelElement)this).Id, _startId, _endId );
        }
    }
}