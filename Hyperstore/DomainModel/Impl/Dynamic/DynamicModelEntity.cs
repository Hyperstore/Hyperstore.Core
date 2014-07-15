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
                                throw new Exception(ExceptionMessages.InvalidValue);
                            }

                            refer = new ReferenceHandler(this, relationship, false);
                            break;
                        }

                        if (relationship.EndPropertyName == propertyName)
                        {
                            if (relationship.Cardinality != Cardinality.OneToMany && relationship.Cardinality != Cardinality.OneToOne)
                            {
                                throw new Exception(ExceptionMessages.InvalidValue);
                            }

                            refer = new ReferenceHandler(this, relationship, true);
                            break;
                        }
                    }

                    _references.TryAdd(propertyName, refer); // adding even refer is null
                    if (refer == null)
                        throw new Exception(string.Format(ExceptionMessages.UnknownPropertyFormat, propertyName));
                }
                else
                {
                    refer = obj as ReferenceHandler;
                    if (refer == null)
                        throw new Exception(ExceptionMessages.InvalidValue);
                }

                if (value != null && !(value is IModelElement))
                    throw new Exception(ExceptionMessages.InvalidValue);

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
                        throw new Exception(string.Format(ExceptionMessages.UnknownPropertyFormat, propertyName));

                    if (relationship.Cardinality == Cardinality.OneToOne || relationship.End.Id == ((IModelElement)this).SchemaInfo.Id) // Noeud terminal
                    {
                        refer = new ReferenceHandler(this, relationship, relationship.Cardinality != Cardinality.OneToOne);
                        _references.TryAdd(propertyName, refer);
                    }
                    else
                    {
                        // TODO create a proxy for enumerable to take into account extensions (convert First() to Enumerable.First(..)) - proxy for Observablecollection should implement inotifycollectionchanged
                        refer = this is INotifyPropertyChanged ? new ObservableModelElementCollection<IModelElement>(this, relationship) : new ModelElementCollection<IModelElement>(this, relationship);
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
    }
}