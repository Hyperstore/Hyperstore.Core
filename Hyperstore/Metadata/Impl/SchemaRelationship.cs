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

namespace Hyperstore.Modeling.Metadata
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema relationship.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.SchemaRelationship"/>
    ///-------------------------------------------------------------------------------------------------
    public class SchemaRelationship<T> : SchemaRelationship where T : IModelRelationship
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  A schema relationship 
        /// </summary>
        /// <param name="start">
        ///  The start element schema
        /// </param>
        /// <param name="end">
        ///  The end element schema
        /// </param>
        /// <param name="cardinality">
        ///  Cardinality of the relationship
        /// </param>
        /// <param name="isembedded">
        ///  Defines if the relationship is embedded
        /// </param>
        /// <param name="name">
        ///  Relationship's name
        /// </param>
        /// <param name="startPropertyName">
        ///  The start property name.
        /// </param>
        /// <param name="endPropertyName">
        ///  The end property name.
        /// </param>
        /// <param name="inheritedSchema">
        ///  (Optional) The schema element inherited by the relationship
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SchemaRelationship(ISchemaElement start, ISchemaElement end, Cardinality cardinality = Cardinality.OneToOne, bool isEmbedded = false, string name = null, string startPropertyName=null, string endPropertyName=null, ISchemaRelationship inheritedSchema = null)
            : base(start, end, typeof(T), cardinality, isEmbedded, name ?? typeof(T).FullName, inheritedSchema, startPropertyName, endPropertyName)
        {
        }
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A schema relationship.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.Metadata.SchemaElement"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISchemaRelationship"/>
    ///-------------------------------------------------------------------------------------------------
    public class SchemaRelationship : SchemaElement, ISchemaRelationship
    {
        private ISchemaElement _start;
        private ISchemaElement _end;
        private Identity _endId;
        private Identity _startId;
        private Cardinality? _cardinality;
        private bool? _isEmbedded;
        private string _startPropertyName;
        private bool _startPropertyNameInitialized;
        private string _endPropertyName;
        private bool _endPropertyNameInitialized;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected SchemaRelationship()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  A schema relationship.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="start">
        ///  The start element schema.
        /// </param>
        /// <param name="end">
        ///  The end element schema.
        /// </param>
        /// <param name="implementedType">
        ///  Type of the implemented.
        /// </param>
        /// <param name="cardinality">
        ///  Cardinality of the relationship.
        /// </param>
        /// <param name="isembedded">
        ///  Defines if the relationship is embedded.
        /// </param>
        /// <param name="name">
        ///  Relationship's name.
        /// </param>
        /// <param name="inheritedSchema">
        ///  (Optional) The schema element inherited by the relationship.
        /// </param>
        /// <param name="startPropertyName">
        ///  The start property name.
        /// </param>
        /// <param name="endPropertyName">
        ///  The end property name.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SchemaRelationship(ISchemaElement start, ISchemaElement end, Type implementedType, Cardinality cardinality, bool isembedded, string name, ISchemaRelationship inheritedSchema = null, string startPropertyName = null, string endPropertyName = null)
        {
            Contract.Requires(start, "start");
            Contract.Requires(end, "end");
            Contract.Requires(implementedType, "implementedType");
            Contract.RequiresNotEmpty(name, "name");

            _start = start;
            _end = end;

            name = Conventions.NormalizeMetaElementName(start.DomainModel.Name, name);
            ConstructInternal(start.DomainModel, implementedType, new Identity(start.Schema.Name, name), name, inheritedSchema, PrimitivesSchema.SchemaRelationshipSchema,
                    (dm, melId, mid) => new AddSchemaRelationshipCommand(dm as ISchema, melId, (ISchemaRelationship)mid, start, end));

            if (!Hyperstore.Modeling.Utils.ReflectionHelper.IsAssignableFrom(typeof(IModelRelationship), ImplementedType))
                throw new Exception("SchemaRelationship must describes a type implementing IModelRelationship");

            Cardinality = cardinality;
            IsEmbedded = isembedded;
            StartId = start.Id;
            EndId = end.Id;
            StartPropertyName = startPropertyName;
            EndPropertyName = endPropertyName;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  A schema relationship 
        /// </summary>
        ///  <param name="name">
        ///  Relationship's name
        /// </param>
        /// <param name="start">
        ///  The start element schema
        /// </param>
        /// <param name="end">
        ///  The end element schema
        /// </param>
        /// </param>
        /// <param name="cardinality">
        ///  Cardinality of the relationship
        /// </param>
        /// <param name="isembedded">
        ///  Defines if the relationship is embedded
        /// </param>
        /// <param name="inheritedSchema">
        ///  (Optional) The schema element inherited by the relationship
        /// </param>
        /// <param name="startPropertyName">
        ///  The start property name.
        /// </param>
        /// <param name="endPropertyName">
        ///  The end property name.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public SchemaRelationship(string name, ISchemaElement start, ISchemaElement end, Cardinality cardinality = Cardinality.OneToOne, bool isembedded = false, ISchemaRelationship inheritedSchema = null, string startPropertyName = null, string endPropertyName = null)
            : this(start, end, typeof(ModelRelationship), cardinality, isembedded, name, inheritedSchema, startPropertyName, endPropertyName)
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the default super class.
        /// </summary>
        /// <value>
        ///  The default super class.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected override ISchemaElement DefaultSuperClass
        {
            get { return PrimitivesSchema.ModelRelationshipSchema; }
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
        }

        bool ISchemaRelationship.IsEmbedded
        {
            get { return IsEmbedded; }
        }

        private bool IsEmbedded
        {
            get
            {
                if (_isEmbedded == null)
                    _isEmbedded = GetPropertyValue<bool>("IsEmbedded");
                return _isEmbedded.Value;
            }
            set
            {
                _isEmbedded = value;
                SetPropertyValue("IsEmbedded", value);
            }
        }

        Cardinality ISchemaRelationship.Cardinality
        {
            get { return Cardinality; }
        }

        private Cardinality Cardinality
        {
            get
            {
                if (_cardinality == null)
                    _cardinality = GetPropertyValue<Cardinality>("Cardinality");
                return _cardinality.Value;
            }
            set
            {
                _cardinality = value;
                SetPropertyValue("Cardinality", value);
            }
        }

        private Identity StartId
        {
            get { return _startId ?? (_startId = GetPropertyValue<Identity>("StartId")); }
            set { SetPropertyValue("StartId", _startId = value); }
        }

        private Identity EndId
        {
            get { return _endId ?? (_endId = GetPropertyValue<Identity>("EndId")); }
            set { SetPropertyValue("EndId", _endId = value); }
        }

        ISchemaElement ISchemaRelationship.Start
        {
            get { return _start ?? (_start = Store.GetSchemaElement(StartId)); }
        }

        ISchemaElement ISchemaRelationship.End
        {
            get { return _end ?? (_end = Store.GetSchemaElement(EndId)); }
        }

        public string StartPropertyName
        {
            get
            {
                if (_startPropertyNameInitialized == false)
                {
                    _startPropertyNameInitialized = true;
                    _startPropertyName = GetPropertyValue<string>("StartPropertyName");
                }
                return _startPropertyName;
            }
            set
            {
                _startPropertyName = value;
                _startPropertyNameInitialized = true;
                if (value != null)
                    SetPropertyValue("StartPropertyName", value);
            }
        }

        public string EndPropertyName
        {
            get
            {
                if (_endPropertyNameInitialized == false)
                {
                    _endPropertyNameInitialized = true;
                    _endPropertyName = GetPropertyValue<string>("EndPropertyName");
                }
                return _endPropertyName;
            }
            set
            {
                _endPropertyName = value;
                _endPropertyNameInitialized = true;
                if( value != null)
                    SetPropertyValue("EndPropertyName", value);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the start.
        /// </summary>
        /// <value>
        ///  The start.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement Start
        {
            get { return ((ISchemaRelationship)this).Start; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the end.
        /// </summary>
        /// <value>
        ///  The end.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ISchemaElement End
        {
            get { return ((ISchemaRelationship)this).End; }
        }

        ISchemaRelationship IModelRelationship.SchemaRelationship
        {
            get { return (ISchemaRelationship)this.Schema; }
        }

        IModelElement IModelRelationship.Start
        {
            get { return Start; }
        }

        IModelElement IModelRelationship.End
        {
            get { return End;  }
        }

        Identity IModelRelationship.EndId
        {
            get { return EndId; }
        }

        Identity IModelRelationship.EndSchemaId
        {
            get { return End.SchemaInfo.Id; }
        }
    }
}