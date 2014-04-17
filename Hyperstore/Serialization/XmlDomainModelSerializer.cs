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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.DomainExtension;
using Hyperstore.Modeling.Metadata;

#endregion

namespace Hyperstore.Modeling.Serialization
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  An XML domain model serializer.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class XmlDomainModelSerializer
    {
        private bool _serializeDomainPropertiesOnly;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="model">
        ///  The model.
        /// </param>
        /// <param name="stream">
        ///  The stream.
        /// </param>
        /// <param name="option">
        ///  (Optional) the option.
        /// </param>
        /// <returns>
        ///  A Task.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public Task Serialize(IDomainModel model, Stream stream, SerializationOption option = SerializationOption.Elements)
        {
            Contract.Requires(model, "model");
            Contract.Requires(stream, "stream");
            var tcs = new TaskCompletionSource<object>();
            try
            {
                ISession session = null;
                if (Session.Current == null)
                {
                    session = model.Store.BeginSession(new SessionConfiguration { Mode = SessionMode.Serializing });
                }
                else
                    Session.Current.SetMode(SessionMode.Serializing);

                try
                {
                    _serializeDomainPropertiesOnly = model is DomainModelExtension && ((DomainModelExtension)model).ExtensionMode == ExtendedMode.Updatable;

                    var root = new XElement("domain", new XAttribute("name", model.Name),
                            //(option & SerializationOption.Metadatas) == SerializationOption.Metadatas
                            //        ? new XElement("metaModel", new XElement("metadatas", SerializeMetaElements(model.Schema)), new XElement("metaRelationships", SerializeMetaRelationships(model.Schema)),
                            //                new XElement("relationships", SerializeRelationships(model.Schema)))
                            //        : null,
                            (option & SerializationOption.Elements) == SerializationOption.Elements ? new XElement("model", new XElement("elements", SerializeElements(model)), new XElement("relationships", SerializeRelationships(model))) : null);

                    using (var writer = XmlWriter.Create(stream))
                    {
                        root.WriteTo(writer);
                    }
                }
                finally
                {
                    if (session != null)
                    {
                        session.AcceptChanges();
                        session.Dispose();
                    }
                } tcs.TrySetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        private IEnumerable<XElement> SerializeRelationships(IDomainModel model)
        {
            foreach (var e in model.GetRelationships()
                    .OrderBy(r => r.Sequence))
            {
                if (e is ISchemaRelationship || e.SchemaInfo.Id == PrimitivesSchema.SchemaElementHasPropertiesSchema.Id || e.SchemaInfo.Id == PrimitivesSchema.SchemaPropertyReferencesSchemaEntitySchema.Id)
                    continue;

                if (e.SchemaInfo == PrimitivesSchema.SchemaElementReferencesSuperElementSchema )
                    continue;

                yield return
                        new XElement("relationship", new XAttribute("id", e.Id), new XAttribute("metadata", GetSchemaIdentity(e.SchemaInfo)),
                            new XAttribute("start", e.Start.Id), new XAttribute("startMetadata", GetSchemaIdentity(e.Start.SchemaInfo)),
                            new XAttribute("end", e.End.Id), new XAttribute("endMetadata", GetSchemaIdentity(e.End.SchemaInfo)), 
                            SerializeAttributes(e));
            }
        }

        private IEnumerable<XElement> SerializeElements(IDomainModel model)
        {
            foreach (var e in model.GetEntities()
                    .OrderBy(r => r.Sequence))
            {
                if (e.SchemaInfo.IsPrimitive || e.SchemaInfo is ISchemaProperty)
                    continue;

                yield return new XElement("element", new XAttribute("id", e.Id), new XAttribute("metadata", GetSchemaIdentity(e.SchemaInfo)), SerializeAttributes(e));
            }
        }

        private XElement SerializeAttributes(IModelElement e)
        {
            XElement props = null;
            var metadata = e.SchemaInfo;

            if (metadata != null)
            {
                foreach (var prop in metadata.GetProperties(!_serializeDomainPropertiesOnly))
                {
                    if (e is ISchemaProperty && (prop.Name == "ImplementedType" || prop.Name == "Name"))
                        continue;

                    if (e is ISchemaElement && (prop.Name == "StartId" || prop.Name == "EndId" || prop.Name == "StartMetaclassId" || prop.Name == "EndMetaclassId"))
                        continue;

                    var pv = e.GetPropertyValue(prop);
                    if (pv == null || pv.Value == null || !pv.HasValue)
                        continue;
                    var value = prop.Serialize(pv.Value);

                    if (props == null)
                        props = new XElement("attributes");

                    props.Add(new XElement("attribute", new XAttribute("name", prop.Name), value));
                }
            }
            return props;
        }

        private Identity GetSchemaIdentity(ISchemaInfo schema)
        {
            return schema.SchemaInfo.IsA(PrimitivesSchema.GeneratedSchemaEntitySchema) ? schema.SchemaInfo.Id : schema.Id;
        }

        private IEnumerable<XElement> SerializeMetaElements(ISchema model)
        {
            foreach (var m in model.GetSchemaInfos().OfType<ISchemaValueObject>()
                                .OrderBy(r => r.Sequence))
            {
                yield return new XElement("metadata", new XAttribute("id", m.Id), new XAttribute("metadata", GetSchemaIdentity(m.SchemaInfo)),
                    new XAttribute("superClass", m.SuperClass.Id),/* SerializeProperties(m),*/ SerializeAttributes(m));
            }

            foreach (var m in model.GetSchemaEntities()
                    .OrderBy(r => r.Sequence))
            {
                //var mid = m.SchemaInfo.SchemaInfo.IsA(PrimitivesSchema.GeneratedSchemaEntitySchema) ? PrimitivesSchema.SchemaEntitySchema.Id : m.SchemaInfo.Id;

                yield return new XElement("metadata", new XAttribute("id", m.Id), new XAttribute("metadata", GetSchemaIdentity(m.SchemaInfo)),
                    new XAttribute("superClass", GetSchemaIdentity(m.SuperClass)), SerializeProperties(m), SerializeAttributes(m));
            }
        }

        private object SerializeProperties(ISchemaElement container)
        {
            if (container == null)
                return null;

            XElement props = null;

            foreach (var prop in container.GetProperties(false))
            {
                if (props == null)
                    props = new XElement("properties");

                props.Add(new XElement("property", // new XAttribute("id", prop.Id),
                        new XAttribute("name", prop.Name), new XAttribute("metadata", prop.PropertySchema.Id), SerializeAttributes(prop)));
                break;
            }

            return props;
        }

        private IEnumerable<XElement> SerializeMetaRelationships(ISchema model)
        {
            foreach (var e in model.GetSchemaRelationships()
                    .OrderBy(r => r.Sequence))
            {
                if (!(e is ISchemaRelationship))
                    continue;

                yield return
                        new XElement("metaRelationship", new XAttribute("id", e.Id), new XAttribute("metadata", e.SchemaInfo.SchemaInfo.Id),
                            new XAttribute("start", e.Start.Id), new XAttribute("startMetadata", GetSchemaIdentity(e.Start.SchemaInfo)),
                            new XAttribute("end", e.End.Id), new XAttribute("endMetadata", GetSchemaIdentity(e.End.SchemaInfo)),
                            new XAttribute("superClass", GetSchemaIdentity(e.SuperClass)), SerializeAttributes(e),
                            SerializeProperties(e as ISchemaElement));
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  A model reader.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public class ModelReader
        {
            private readonly Func<string, string, IDomainModel> _modelFactory;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  The model.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public IDomainModel Model;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="modelFactory">
            ///  The model factory.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public ModelReader(Func<string, string, IDomainModel> modelFactory)
            {
                _modelFactory = modelFactory;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="domainModel">
            ///  The domain model.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public ModelReader(IDomainModel domainModel)
            {
                Model = domainModel;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Reads meta model.
            /// </summary>
            /// <param name="stream">
            ///  The stream.
            /// </param>
            /// <param name="loadModelOnly">
            ///  true to load model only.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public void ReadMetaModel(Stream stream, bool loadModelOnly)
            {
                Contract.Requires(stream, "stream");
                stream.Position = 0;
                using (var reader = XmlReader.Create(stream))
                {
                    var inMetaModel = false;
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (!inMetaModel)
                            {
                                if (reader.LocalName == "metaModel")
                                    inMetaModel = true;
                                else if (reader.LocalName == "domain")
                                {
                                    OnReadDomain(reader);
                                    if (loadModelOnly)
                                        break;
                                }
                                continue;
                            }

                            switch (reader.LocalName)
                            {
                                case "metadata":
                                    OnMetadata(reader.ReadSubtree());
                                    break;
                                case "metaRelationships":
                                    OnMetaRelationship(reader.ReadSubtree());
                                    break;
                                case "relationship":
                                    OnRelationship(reader.ReadSubtree());
                                    break;
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "metaModel")
                            break;
                    }
                }
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Reads a model.
            /// </summary>
            /// <param name="stream">
            ///  The stream.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public void ReadModel(Stream stream)
            {
                Contract.Requires(stream, "stream");

                stream.Position = 0;
                using (var reader = XmlReader.Create(stream))
                {
                    var inModel = false;
                    while (reader.Read())
                    {

                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (!inModel)
                            {
                                if (reader.LocalName == "model")
                                    inModel = true;

                                continue;
                            }

                            switch (reader.LocalName)
                            {
                                case "element":
                                    OnElement(reader.ReadSubtree());
                                    break;
                                case "relationship":
                                    OnRelationship(reader.ReadSubtree());
                                    break;
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "model")
                            break;
                    }
                }
            }

            private void OnElement(XmlReader reader)
            {
                IModelElement current = null;

                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (reader.LocalName == "element")
                    {
                        var mid = Identity.Parse(reader.GetAttribute("metadata"));
                        var id = Identity.Parse(reader.GetAttribute("id"));
                        var metadata = Model.Store.GetSchemaEntity(mid);
                        current = Model.GetElement(id, metadata);
                        if (current == null)
                        {
                            Session.Current.Execute(new AddEntityCommand(Model, metadata, id));
                            current = Model.GetElement(id, metadata);
                            Debug.Assert(current != null);
                        }
                    }
                    else if (current != null && reader.LocalName == "attribute")
                        ReadAttributes(current, reader.ReadSubtree());
                    else if (current != null && reader.LocalName == "property")
                        ReadProperties(reader.ReadSubtree(), current as ISchemaElement);
                }

                while (reader.Read())
                {
                }
            }

            private void OnRelationship(XmlReader reader)
            {
                IModelElement current = null;

                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (reader.LocalName == "relationship")
                    {
                        var mid = Identity.Parse(reader.GetAttribute("metadata"));
                        var id = Identity.Parse(reader.GetAttribute("id"));

                        var startMetaclassId = Identity.Parse(reader.GetAttribute("startMetadata"));
                        var endMetaclassId = Identity.Parse(reader.GetAttribute("endMetadata"));

                        var startId = Identity.Parse(reader.GetAttribute("start"));
                        var endId = Identity.Parse(reader.GetAttribute("end"));

                        var startMetadata = Model.Store.GetSchemaElement(startMetaclassId);
                        var start = Model.Store.GetElement(startId, startMetadata);
                        if (start == null)
                            throw new InvalidElementException(startId);

                        var endMetadata = Model.Store.GetSchemaElement(endMetaclassId);
                        var end = Model.Store.GetElement(endId, endMetadata);
                        if (end == null)
                            throw new InvalidElementException(endId);

                        var metadata = Model.Store.GetSchemaRelationship(mid);
                        Session.Current.Execute(new AddRelationshipCommand(metadata, start, end, id));
                        current = start.DomainModel.GetRelationship(id, metadata);
                    }
                    else if (current != null && reader.LocalName == "attribute")
                        ReadAttributes(current, reader.ReadSubtree());
                    else if (current != null && reader.LocalName == "property")
                        ReadProperties(reader.ReadSubtree(), current as ISchemaElement);
                }
            }

            private void OnMetaRelationship(XmlReader reader)
            {
                IModelElement current = null;
                if (!reader.Read())
                    return;

                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (reader.LocalName == "metaRelationship")
                    {
                        var mid = Identity.Parse(reader.GetAttribute("metadata"));
                        var id = Identity.Parse(reader.GetAttribute("id"));
                        var metadata = Model.Store.GetSchemaRelationship(mid, false) ?? PrimitivesSchema.SchemaRelationshipSchema;
                        if (Model.Store.GetRelationship(id, metadata) != null)
                            break;

                        var startId = Identity.Parse(reader.GetAttribute("start"));
                        var endId = Identity.Parse(reader.GetAttribute("end"));
                        var start = Model.Store.GetSchemaElement(startId);
                        if (start == null)
                            throw new InvalidElementException(startId);
                        var end = Model.Store.GetSchemaElement(endId);
                        if (end == null)
                            throw new InvalidElementException(endId);

                        Session.Current.Execute(new AddSchemaRelationshipCommand(Model as ISchema, id, metadata, start, end));
                        current = Model.Store.GetSchemaRelationship(id);
                    }
                    else if (reader.LocalName == "attribute")
                        ReadAttributes(current, reader.ReadSubtree());
                    else if (current != null && reader.LocalName == "property")
                        ReadProperties(reader.ReadSubtree(), current as ISchemaElement);
                }
                while (reader.Read())
                {
                }
            }

            private void OnMetadata(XmlReader reader)
            {
                ISchemaInfo current = null;

                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (reader.LocalName == "metadata")
                    {
                        var mid = Identity.Parse(reader.GetAttribute("metadata"));
                        var id = Identity.Parse(reader.GetAttribute("id"));
                        var metadata = Model.Store.GetSchemaEntity(mid);

                        var superTypeId = Identity.Parse(reader.GetAttribute("superClass"));
                        this.Model.Store.GetSchemaEntity(superTypeId);

                        current = Model.Store.GetSchemaEntity(id, false);
                        if (current == null)
                        {
                            Session.Current.Execute(new AddSchemaEntityCommand(Model as ISchema, id, metadata));
                            // TODO il faut pouvoir rajouter le supertype ici
                            current = Model.Store.GetSchemaInfo(id);
                            //break;
                        }
                    }
                    else if (current != null && reader.LocalName == "attribute")
                        ReadAttributes(current, reader.ReadSubtree());
                    else if (current != null && reader.LocalName == "property")
                        ReadProperties(reader.ReadSubtree(), current as ISchemaElement);
                }

                while (reader.Read())
                {
                }
            }

            private void OnReadDomain(XmlReader reader)
            {
                if (Model == null)
                {
                    var name = reader.GetAttribute("name");
                    var metaName = reader.GetAttribute("metaName");
                    Model = _modelFactory(name, metaName);
                }
            }

            private void ReadProperties(XmlReader reader, ISchemaElement parent)
            {
                IModelElement current = null;
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (reader.LocalName == "property")
                    {
                        var mid = Identity.Parse(reader.GetAttribute("metadata"));
                        var name = reader.GetAttribute("name");
                        var metadata = Model.Store.GetSchemaInfo(mid, false) as ISchemaValueObject;

                        current = parent.DefineProperty(name, metadata);
                    }
                    else if (current != null && reader.LocalName == "attribute")
                        ReadAttributes(current, reader.ReadSubtree());
                }
            }

            private void ReadAttributes(IModelElement current, XmlReader reader)
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (reader.LocalName == "attribute")
                    {
                        var propertyName = reader.GetAttribute("name");
                        reader.Read();
                        var val = reader.Value;
                        if (String.IsNullOrWhiteSpace(val))
                            val = null;
                        var property = current.SchemaInfo.GetProperty(propertyName);
                        if (property == null)
                            throw new Exception(string.Format(ExceptionMessages.UnknownPropertyForElementFormat,propertyName,current.Id));

                        var propMetadata = property.PropertySchema;
                        var cmd = new ChangePropertyValueCommand(current, property, propMetadata.Deserialize(new SerializationContext(Model, current.Id, propMetadata, val)));
                        Session.Current.Execute(cmd);
                    }
                }
            }
        }

        #region Deserialize

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="store">
        ///  The store.
        /// </param>
        /// <param name="stream">
        ///  The stream.
        /// </param>
        /// <param name="domainName">
        ///  (Optional) name of the domain.
        /// </param>
        /// <param name="desc">
        ///  (Optional) the description.
        /// </param>
        /// <returns>
        ///  An IDomainModel.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel Deserialize(IHyperstore store, Stream stream, string domainName = null, ISchemaDefinition desc = null)
        {
            Contract.Requires(store, "store");
            Contract.Requires(stream, "stream");
            throw new NotImplementedException();

            //var r = new ModelReader((name, metaName) =>  store.LoadDomainModel(domainName ?? name, desc ?? new EmptyDomainModelDefinition(metaName)));

            //using (var scope = store.BeginSession())
            //{
            //    r.ReadMetaModel(stream, desc != null);
            //    scope.AcceptChanges();
            //}

            //using (var scope = store.BeginSession())
            //{
            //    r.ReadModel(stream);
            //    scope.AcceptChanges();
            //}
            //return r.Model;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  true this instance to the given stream.
        /// </summary>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        /// <param name="stream">
        ///  The stream.
        /// </param>
        /// <returns>
        ///  An IDomainModel.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IDomainModel Deserialize(IDomainModel domainModel, Stream stream)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(stream, "stream");
            var r = new ModelReader(domainModel);

            using (var scope = domainModel.Store.BeginSession())
            {
                r.ReadModel(stream);
                scope.AcceptChanges();
            }
            return r.Model;
        }

        #endregion
    }
}