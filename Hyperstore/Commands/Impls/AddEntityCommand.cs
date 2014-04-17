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
using Hyperstore.Modeling.Events;

#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An add entity command.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.PrimitiveCommand"/>
    /// <seealso cref="T:Hyperstore.Modeling.Commands.ICommandHandler{Hyperstore.Modeling.Commands.AddEntityCommand}"/>
    ///-------------------------------------------------------------------------------------------------
    public class AddEntityCommand : PrimitiveCommand, ICommandHandler<AddEntityCommand>
    {
        private IModelEntity _element;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddEntityCommand" /> class.
        /// </summary>
        /// <param name="mel">The mel.</param>
        internal AddEntityCommand(IModelEntity mel)
            : this(mel.DomainModel, mel.SchemaInfo as ISchemaEntity, mel.Id)
        {
            _element = mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Initializes a new instance of the <see cref="AddEntityCommand" /> class.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="schemaEntity">
        ///  The schema entity.
        /// </param>
        /// <param name="id">
        ///  (Optional) The identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public AddEntityCommand(IDomainModel domainModel, ISchemaEntity schemaEntity, Identity id = null)
            : base(domainModel)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(schemaEntity, "schemaEntity");

            Id = id ?? domainModel.IdGenerator.NextValue(schemaEntity);
            if (String.Compare(Id.DomainModelName, domainModel.Name, StringComparison.OrdinalIgnoreCase) != 0)
                throw new Exception("The id must be an id of the specified domain model.");

            SchemaEntity = schemaEntity;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the identifier.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entity.
        /// </summary>
        /// <value>
        ///  The schema entity.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaEntity SchemaEntity { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Element created.
        /// </summary>
        /// <value>
        ///  The entity.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IModelEntity Entity
        {
            get { return _element ?? (_element = DomainModel.GetEntity(Id, SchemaEntity)); }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Handles the given context.
        /// </summary>
        /// <param name="context">
        ///  The context.
        /// </param>
        /// <returns>
        ///  An IEvent.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEvent Handle(ExecutionCommandContext<AddEntityCommand> context)
        {
            DebugContract.Requires(context);

            var dm = DomainModel as IUpdatableDomainModel;
            if (dm == null)
                return null;

            using (CodeMarker.MarkBlock("AddEntityCommand.Handle"))
            {
                _element = dm.CreateEntity(Id, SchemaEntity, _element) as IModelEntity;
            }
            return new AddEntityEvent(DomainModel.Name, DomainModel.ExtensionName, Id, SchemaEntity.Id, context.CurrentSession.SessionId, 1);
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
            return String.Format("Add {0} element", Id);
        }
    }
}