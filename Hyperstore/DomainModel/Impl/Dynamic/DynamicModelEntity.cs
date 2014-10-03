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
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Hyperstore.Modeling.Platform;

#endregion

namespace Hyperstore.Modeling
{
    // http://msdn.microsoft.com/fr-fr/vcsharp/ff800651.aspx 
    /// <summary>
    ///     Dynamic element.
    /// </summary>
    public class DynamicModelEntity : ModelEntity, IDynamicMetaObjectProvider, INotifyPropertyChanged
    {
        private IConcurrentDictionary<string, object> _references;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Construoctor taken a metadata name.
        /// </summary>
        /// <remarks>
        ///  Une nouvelle méta donnée sera crée avec le nom passé en paramètre si il n'esiste pas.
        /// </remarks>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="schemaName">
        ///  Name of the schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DynamicModelEntity(IDomainModel domainModel, string schemaName)
            : this(domainModel, domainModel.Store.GetSchemaEntity(schemaName))
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructeur.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="schemaEntity">
        ///  The schema entity.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DynamicModelEntity(IDomainModel domainModel, ISchemaEntity schemaEntity)
            : base(domainModel, schemaEntity)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(schemaEntity != null && !schemaEntity.Id.IsEmpty, "schemaEntity");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Méthode appelée dans le processus de création que celle ci soit faite par new ou par
        ///  sérialisation.
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema container.
        /// </param>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected override void Initialize(ISchemaElement schemaElement, IDomainModel domainModel)
        {
            base.Initialize(schemaElement, domainModel);
            _references = PlatformServices.Current.CreateConcurrentDictionary<string, object>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns the <see cref="T:System.Dynamic.DynamicMetaObject" /> responsible for binding
        ///  operations performed on this object.
        /// </summary>
        /// <param name="parameter">
        ///  The parameter.
        /// </param>
        /// <returns>
        ///  The <see cref="T:System.Dynamic.DynamicMetaObject" /> to bind this object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicModelElementMetaObject(parameter, this);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Try set property.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public object TrySetProperty(string propertyName, object value)
        {
            var property = ((IModelElement)this).SchemaInfo.GetProperty(propertyName);
            if (property == null)
            {
                ReferenceHandler refer = null;
                object obj;
                if (!_references.TryGetValue(propertyName, out obj))
                {

                    // Find a relationship whith this name
                    foreach (ISchemaRelationship relationship in ((IModelElement)this).SchemaInfo.GetRelationships())
                    {
                        if (relationship.StartPropertyName == propertyName)
                        {
                            if (relationship.Cardinality != Cardinality.OneToOne && relationship.Cardinality != Cardinality.ManyToOne)
                            {
                                throw new HyperstoreException(ExceptionMessages.InvalidValue);
                            }

                            refer = new ReferenceHandler(this, relationship, false);
                            break;
                        }

                        if (relationship.EndPropertyName == propertyName)
                        {
                            if (relationship.Cardinality != Cardinality.OneToMany && relationship.Cardinality != Cardinality.OneToOne)
                            {
                                throw new HyperstoreException(ExceptionMessages.InvalidValue);
                            }

                            refer = new ReferenceHandler(this, relationship, true);
                            break;
                        }
                    }

                    _references.TryAdd(propertyName, refer); // adding even refer is null
                    if (refer == null)
                        throw new Hyperstore.Modeling.Metadata.PropertyDefinitionException(string.Format(ExceptionMessages.UnknownPropertyFormat, propertyName));
                }
                else
                {
                    refer = obj as ReferenceHandler;
                    if (refer == null)
                        throw new HyperstoreException(ExceptionMessages.InvalidValue);
                }

                if (value != null && !(value is IModelElement))
                    throw new HyperstoreException(ExceptionMessages.InvalidValue);

                var mel = value as IModelElement;

                ((ReferenceHandler)refer).SetReference(mel);
                return value;
            }

            SetPropertyValue(property, value);
            return value;
        }

        private static bool IsMatchPropertyName(IModelRelationship relationship, string propertyName)
        {
            // TODO a reprendre completement pour utiliser 
            // relationship.SchemaRelationship.StartPropertyName ou end
            var name = relationship.Id.Key;
            if (String.Compare(name, propertyName, StringComparison.Ordinal) == 0)
                return true;

            var sourceName = Types.SplitFullName(relationship.Start.Id.Key).Item2;
            if (name.StartsWith(sourceName, StringComparison.Ordinal))
            {
                name = name.Substring(sourceName.Length);
                var cases = new[] { "Has" + propertyName, "References" + propertyName, propertyName };

                if (cases.Any(c => String.Compare(name, c, StringComparison.Ordinal) == 0))
                    return true;
            }
            return false;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Try get property.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <returns>
        ///  An object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public object TryGetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "Id":
                    return ((IModelElement)this).Id;
                case "DomainModel":
                    return DomainModel;
                case "Schema":
                    return ((IModelElement)this).SchemaInfo;
                case "Store":
                    return Store;
            }

            var property = ((IModelElement)this).SchemaInfo.GetProperty(propertyName);
            if (property == null)
            {
                object refer;
                if (!_references.TryGetValue(propertyName, out refer))
                {
                    // Find a relationship whith this name
                    var relationship = ((IModelElement)this).SchemaInfo.GetRelationships()
                            .FirstOrDefault(r => IsMatchPropertyName(r, propertyName)) as ISchemaRelationship;
                    if (relationship == null)
                        throw new Hyperstore.Modeling.Metadata.PropertyDefinitionException(string.Format(ExceptionMessages.UnknownPropertyFormat, propertyName));

                    if (relationship.Cardinality == Cardinality.OneToOne || relationship.End.Id == ((IModelElement)this).SchemaInfo.Id) // Noeud terminal
                    {
                        refer = new ReferenceHandler(this, relationship, relationship.Cardinality != Cardinality.OneToOne);
                        _references.TryAdd(propertyName, refer);
                    }
                    else
                    {
                        // TODO create a proxy for enumerable to take into account extensions (convert First() to Enumerable.First(..)) - proxy for Observablecollection should implement inotifycollectionchanged
                        var isObservable = this is INotifyPropertyChanged && (((IModelElement)this).SchemaInfo.Schema.Behavior & DomainBehavior.Observable) != DomainBehavior.Observable;
                        if (isObservable)
                            refer = new ObservableModelElementCollection<IModelElement>(this, relationship);
                        else
                            refer = new ModelElementCollection<IModelElement>(this, relationship);
                        _references.TryAdd(propertyName, refer);
                    }
                }

                var handler = refer as ReferenceHandler;
                if (handler != null)
                    return handler.GetReference();

                return refer;
            }

            var pv = GetPropertyValue(property);
            return pv.Value;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the Hyperstore.Modeling.ModelElement and optionally
        ///  releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///  true to release both managed and unmanaged resources; false to release only unmanaged
        ///  resources.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var r in _references)
            {
                var disposable = r.Value as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            _references = null;
        }
    }
}