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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Validations;
using Hyperstore.Modeling.Platform;

#endregion

namespace Hyperstore.Modeling.Metadata
{
    [DebuggerDisplay("Meta Model {Name}")]
    internal class DomainSchema : DomainModel, ISchema, IUpdatableSchema
    {
        private IConstraintsManager _constraints;

        private IConcurrentDictionary<Identity, IModelElement> _elements;
        private IConcurrentDictionary<Identity, IModelRelationship> _relationships;
        private readonly DomainBehavior _behavior;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="dependencyResolver">
        ///  The dependency resolver.
        /// </param>
        /// <param name="constraints">
        ///  (Optional)
        ///  The constraints.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainSchema(string name, IDependencyResolver dependencyResolver, DomainBehavior behavior= DomainBehavior.DisableL1Cache, IConstraintsManager constraints = null)
            : base(dependencyResolver, name)
        {
            Contract.Requires(dependencyResolver, "dependencyResolver");

            _elements = PlatformServices.Current.CreateConcurrentDictionary<Identity, IModelElement>();
            _relationships = PlatformServices.Current.CreateConcurrentDictionary<Identity, IModelRelationship>();
            _behavior = behavior;

            _constraints = constraints ?? dependencyResolver.Resolve<IConstraintsManager>();
            if (_constraints is IDomainService)
                ((IDomainService)_constraints).SetDomain(this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets domain behaviors.
        /// </summary>
        /// <value>
        ///  The behavior.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public DomainBehavior Behavior { get { return _behavior; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the constraints.
        /// </summary>
        /// <value>
        ///  The constraints.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintsManager Constraints
        {
            get { return _constraints; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Configures this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public virtual System.Threading.Tasks.Task Initialize(ISchemaDefinition definition)
        {
            ((IDomainModel)this).Configure();

            SchemaDefinition = definition;
            if (SchemaDefinition != null)
            {
                SchemaDefinition.DefineSchema(this);
                SchemaDefinition.OnSchemaLoaded(this);
            }
            return Hyperstore.Modeling.Utils.CompletedTask.Default;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Loads schema extension.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="definition">
        ///  The definition.
        /// </param>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        /// <returns>
        ///  The schema extension.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public System.Threading.Tasks.Task<ISchema> LoadSchemaExtension(ISchemaDefinition definition, SchemaConstraintExtensionMode mode)
        {
            Contract.Requires(definition, "definition");

            if ((Store.Options & StoreOptions.EnableExtensions) != StoreOptions.EnableExtensions)
                throw new Exception("Extensions are not enabled. Use StoreOptions.EnableExtensions when instancing the store.");

            if (String.CompareOrdinal(definition.SchemaName, this.Name) == 0)
                throw new Exception("Extension schema must have the same name that the extended schema.");

            var desc = new ExtensionSchemaDefinition(definition, this, mode);
            return Store.LoadSchemaAsync(desc);
        }

        protected override void CreateCache()
        {
            // No cache
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            var disposable = _constraints as IDisposable;
            if (disposable != null)
                disposable.Dispose();
            _constraints = null;

            foreach(var kv in _elements)
            {
                disposable = kv.Value as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            _elements = null;
            
            foreach (var kv in _relationships)
            {
                disposable = kv.Value as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            _relationships = null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema definition.
        /// </summary>
        /// <value>
        ///  The schema definition.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaDefinition SchemaDefinition { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaInfo GetSchemaInfo(Identity id, bool throwErrorIfNotExists = true)
        {
            Contract.Requires(id, "id");

            var mel = GetElement(id, null, true) as ISchemaInfo;
            if (mel == null && throwErrorIfNotExists)
                throw new MetadataNotFoundException(id.ToString());
            return mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadata.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema information.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaInfo GetSchemaInfo(string name, bool throwErrorIfNotExists = true)
        {
            Contract.RequiresNotEmpty(name, "name");

            var id = new Identity(Name, Conventions.NormalizeMetaElementName(this.Name, name));
            return GetSchemaInfo(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
        /// </summary>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaEntity GetSchemaEntity(Identity id, bool throwErrorIfNotExists = true)
        {
            Contract.Requires(id, "id");


            var mel = GetElement(id, null, true) as ISchemaEntity;
            if (mel == null && throwErrorIfNotExists)
                throw new MetadataNotFoundException(id.ToString());
            return mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaEntity GetSchemaEntity<T>(bool throwErrorIfNotExists = true) where T : IModelEntity
        {
            var id = new Identity(Name, Conventions.NormalizeMetaElementName(this.Name, typeof(T).FullName));
            return GetSchemaEntity(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema entity.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaEntity GetSchemaEntity(string name, bool throwErrorIfNotExists = true)
        {
            Contract.RequiresNotEmpty(name, "name");

            var id = new Identity(Name, Conventions.NormalizeMetaElementName(this.Name, name));
            return GetSchemaEntity(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaElement GetSchemaElement(string name, bool throwErrorIfNotExists = true)
        {
            Contract.RequiresNotEmpty(name, "name");

            var id = new Identity(Name, Conventions.NormalizeMetaElementName(this.Name, name));
            return GetSchemaElement(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets schema element.
        /// </summary>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaElement GetSchemaElement(Identity id, bool throwErrorIfNotExists = true)
        {
            Contract.Requires(id, "id");


            var mel = GetElement(id, null, true) as ISchemaElement;
            if (mel == null && throwErrorIfNotExists)
                throw new MetadataNotFoundException(id.ToString());
            return mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema elements in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema elements in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEnumerable<ISchemaElement> GetSchemaElements()
        {
            return GetSchemaInfos()
                    .OfType<ISchemaElement>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the schema entities in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema entities in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEnumerable<ISchemaEntity> GetSchemaEntities()
        {
            return GetSchemaInfos()
                    .OfType<ISchemaEntity>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the metadatas.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema infos in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEnumerable<ISchemaInfo> GetSchemaInfos()
        {
            return GetElements()
                    .Where(g => g.SchemaInfo != PrimitivesSchema.GeneratedSchemaEntitySchema)
                    .OfType<ISchemaInfo>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaRelationship GetSchemaRelationship<T>(bool throwErrorIfNotExists = true) where T : IModelRelationship
        {
            var id = new Identity(Name, Conventions.NormalizeMetaElementName(this.Name, typeof(T).FullName));
            return GetSchemaRelationship(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <exception cref="MetadataNotFoundException">
        ///  Thrown when a Metadata Not Found error condition occurs.
        /// </exception>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaRelationship GetSchemaRelationship(Identity id, bool throwErrorIfNotExists = true)
        {
            Contract.Requires(id, "id");

            var rel = GetRelationship(id, null, true) as ISchemaRelationship;
            if (rel == null && throwErrorIfNotExists)
                throw new MetadataNotFoundException(id.ToString());
            return rel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationship.
        /// </summary>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <param name="throwErrorIfNotExists">
        ///  (Optional) true to throw error if not exists.
        /// </param>
        /// <returns>
        ///  The schema relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISchemaRelationship GetSchemaRelationship(string name, bool throwErrorIfNotExists = true)
        {
            Contract.RequiresNotEmpty(name, "name");

            var id = new Identity(Name, Conventions.NormalizeMetaElementName(this.Name, name));
            return GetSchemaRelationship(id, throwErrorIfNotExists);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta relationships.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the schema relationships in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEnumerable<ISchemaRelationship> GetSchemaRelationships()
        {
            return GetSchemaInfos()
                    .OfType<ISchemaRelationship>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets an element.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  The metaclass.
        /// </param>
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The element.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IModelElement GetElement(Identity id, ISchemaElement metaclass, bool localOnly = true)
        {
            Contract.Requires(id, "id");

            IModelElement mel;
            if (_elements.TryGetValue(id, out mel))
                return mel;

            mel = InnerGraph.GetElement(id, metaclass, localOnly);
            if (mel != null)
                _elements.TryAdd(id, mel);

            return mel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a relationship.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="metaclass">
        ///  the metadata.
        /// </param>
        /// <param name="localOnly">
        ///  (Optional) true to local only.
        /// </param>
        /// <returns>
        ///  The relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IModelRelationship GetRelationship(Identity id, ISchemaRelationship metaclass, bool localOnly = true)
        {
            Contract.Requires(id, "id");

            IModelRelationship mel;
            if (_relationships.TryGetValue(id, out mel))
                return mel;

            mel = InnerGraph.GetRelationship(id, metaclass, localOnly);
            if (mel != null)
                _relationships.TryAdd(id, mel);

            return mel;
        }

        void IUpdatableSchema.AddEntitySchema(Identity id, ISchemaEntity metaclass)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaclass, "metaclass");

            if (string.Compare(id.DomainModelName, this.Name, StringComparison.OrdinalIgnoreCase) != 0)
                throw new Exception(string.Format(ExceptionMessages.DomainNameMismatchFormat, id, Name));

            using (EnsuresRunInSession())
            {
                ((IUpdatableDomainModel)this).CreateEntity(id, metaclass);
            }
        }

        void IUpdatableSchema.AddPropertySchema(Identity id, ISchemaEntity metaclass)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaclass, "metaclass");

            ((IUpdatableDomainModel)this).CreateEntity(id, metaclass);
        }

        void IUpdatableSchema.AddRelationshipSchema(Identity id, ISchemaRelationship metaclass, ISchemaElement start, ISchemaElement end)
        {
            Contract.Requires(id, "id");
            Contract.Requires(metaclass, "metaclass");
            Contract.Requires(start, "start");
            Contract.Requires(end, "end");

            if (string.Compare(id.DomainModelName, this.Name, StringComparison.OrdinalIgnoreCase) != 0)
                throw new Exception(string.Format(ExceptionMessages.DomainNameMismatchFormat, id, Name));

            using (var session = EnsuresRunInSession())
            {
                ((IUpdatableDomainModel)this).CreateRelationship(id, metaclass, start, end.Id, end.SchemaInfo);

                if (session != null)
                    session.AcceptChanges();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve identifier generator.
        /// </summary>
        /// <returns>
        ///  An IIdGenerator.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected override IIdGenerator ResolveIdGenerator()
        {
            return new GuidIdGenerator();
        }
    }
}