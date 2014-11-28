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
using System.Collections.Generic;
using System.Linq;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Utils;
using Hyperstore.Modeling.HyperGraph;
using Hyperstore.Modeling.Platform;

#endregion

namespace Hyperstore.Modeling.Domain
{
    internal interface ICacheAccessor
    {
        IModelElement TryGetFromCache(Identity id);
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A level 1 cache.
    /// </summary>
    /// <seealso cref="T:System.IDisposable"/>
    ///-------------------------------------------------------------------------------------------------
    internal sealed class Level1Cache : IDisposable
    {
        private IConcurrentDictionary<Identity, IModelElement> _cache;
        private IHyperGraphProvider _domain;

        private IHyperGraph InnerGraph
        {
            get { if (_domain == null) throw new UnloadedDomainException("Cannot access an element from an unloaded domain"); return _domain.InnerGraph; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public Level1Cache(DomainModel domain)
        {
            DebugContract.Requires(domain);

            _cache = PlatformServices.Current.CreateConcurrentDictionary<Identity, IModelElement>();
            _domain = domain;

            InnerGraph.DomainModel.Store.SessionCreated += OnSessionCreated;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        ///  resources.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            if (_domain != null)
            {
                InnerGraph.DomainModel.Store.SessionCreated -= OnSessionCreated;
                _domain = null;
                _cache = null;
            }
        }

        private void OnSessionCreated(object sender, SessionCreatedEventArgs e)
        {
            e.Session.Completing += OnSessionCompleting;
        }

        private void OnSessionCompleting(object sender, SessionCompletingEventArgs e)
        {
            if (e.Session.IsAborted)
                return;

            foreach (var elem in e.Session.TrackingData.GetTrackedElementsByState(TrackingState.Removed))
            {
                IModelElement weak;
                if (_cache.TryRemove(elem.Id, out weak) && weak is IDisposable)
                {
                    ((IDisposable)weak).Dispose();
                }
            }
        }

        internal IModelElement GetElement(Identity id)
        {
            DebugContract.Requires(id);
            IModelElement elem;

            var cacheEnabled = (Session.Current == null || (Session.Current.Mode & SessionMode.IgnoreCache) == SessionMode.IgnoreCache);

            if (cacheEnabled)
            {
                elem = TryGetFromCache(id);
                if (elem != null)
                {
                    if (InnerGraph.IsDeleted(id))
                        return null;

                    if (Session.Current != null && Session.Current.TrackingData.GetTrackedElementState(elem.Id) == TrackingState.Removed)
                        return null;

                    return elem;
                }
            }

            elem = InnerGraph.GetElement(id);
            if (elem != null && Session.Current != null && Session.Current.TrackingData.GetTrackedElementState(elem.Id) == TrackingState.Removed)
                return null;

            if (cacheEnabled && elem != null)
            {
                _cache.TryAdd(id, elem);
            }

            return elem;
        }

        internal IModelElement TryGetFromCache(Identity id)
        {
            IModelElement elem;
            _cache.TryGetValue(id, out elem);
            return elem;
        }


        private IModelElement AddElement(IModelElement instance)
        {
            DebugContract.Requires(instance);

            if ( (Session.Current != null && (Session.Current.Mode & SessionMode.IgnoreCache) == SessionMode.IgnoreCache))
                return instance;

            return _cache.GetOrAdd(instance.Id, instance);
            //var mel = val as IModelElement;
            //if (mel != null)
            //    return mel;

            //val = instance;
            //// To ensure data was not removed after the last GetOrAdd
            //val = _cache.GetOrAdd(instance.Id, val);
            //return val as IModelElement;
        }

        internal IModelElement CreateEntity(Identity id, ISchemaEntity metaClass, IModelEntity instance)
        {
            IModelElement mel = instance;
            var r = InnerGraph.CreateEntity(id, metaClass);
            if (instance == null)
            {
                mel = (IModelElement)metaClass.Deserialize(new SerializationContext(_domain, metaClass, r));
            }
            AddElement(mel);

            return mel;
        }

        internal IModelRelationship CreateRelationship(Identity id, ISchemaRelationship relationshipSchema, IModelElement start, Identity endId,  IModelRelationship relationship)
        {
            var r = InnerGraph.CreateRelationship(id, relationshipSchema, start.Id, endId);

            if (relationship == null)
            {
                relationship = (IModelRelationship)relationshipSchema.Deserialize(new SerializationContext(_domain, relationshipSchema, r));
            }
            AddElement(relationship);

            return relationship;
        }

        internal IEnumerable<IModelEntity> GetEntities(ISchemaEntity metaClass, int skip)
        {
            foreach (var e in InnerGraph.GetEntities(metaClass, skip))
            {
                // TODO voir commentaire dans DomainModelExtension.CreateCache
                // Dans le cas d'une extension de de domaine, il faut s'assurer de ne conserver dans le cache que les instances
                // d'un type du domaine sous peine d'avoir des casts invalides si le domaine est déchargé
                yield return (IModelEntity)AddElement(e);
            }
        }

        internal IEnumerable<IModelElement> GetElements(ISchemaElement metaClass, int skip)
        {
            foreach (var e in InnerGraph.GetElements(metaClass, skip))
            {
                // TODO voir commentaire dans DomainModelExtension.CreateCache
                // Dans le cas d'une extension de de domaine, il faut s'assurer de ne conserver dans le cache que les instances
                // d'un type du domaine sous peine d'avoir des casts invalides si le domaine est déchargé
                yield return AddElement(e);
            }
        }

        internal IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship metadata = null, IModelElement start = null, IModelElement end = null, int skip = 0)
        {
            foreach (var e in InnerGraph.GetRelationships(metadata, start, end, skip))
            {
                yield return (IModelRelationship)AddElement(e);
            }
        }
    }
}