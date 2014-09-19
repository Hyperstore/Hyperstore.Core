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
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling.Platform
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A platform services.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public class PlatformServices
    {
        private static PlatformServices _instance;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised default constructor for use only by derived classes.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected PlatformServices()
        {

        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the current instance.
        /// </summary>
        /// <value>
        ///  The current.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public static PlatformServices Current { get { if (_instance == null) { _instance = new PlatformServices(); } return _instance; } protected set { DebugContract.Requires(value); _instance = value; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates synchonization context.
        /// </summary>
        /// <returns>
        ///  The new synchonization context.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual Hyperstore.Modeling.ISynchronizationContext CreateSynchonizationContext() { return new EmptyDispatcher(); }

        private ICodeMarkerListener _codeMarkerListener;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the code marker listener.
        /// </summary>
        /// <value>
        ///  The code marker listener.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public ICodeMarkerListener CodeMarkerListener
        {
            get
            {
                return _codeMarkerListener ?? (_codeMarkerListener = CreateCodeMarkerListener());
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates code marker listener.
        /// </summary>
        /// <returns>
        ///  The new code marker listener.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ICodeMarkerListener CreateCodeMarkerListener()
        {
            return new DefaultCodeMarkerListener();
        }

        private IJsonSerializer _objectSerializer;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the object serializer.
        /// </summary>
        /// <value>
        ///  The object serializer.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public IJsonSerializer ObjectSerializer
        {
            get
            {
                return _objectSerializer ?? (_objectSerializer = CreateObjectSerializer());
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates object serializer.
        /// </summary>
        /// <returns>
        ///  The new object serializer.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IJsonSerializer CreateObjectSerializer() { return new JSonSerializer(); }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates transaction scope.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="cfg">
        ///  The configuration.
        /// </param>
        /// <returns>
        ///  The new transaction scope.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual Commands.ITransactionScope CreateTransactionScope(Session session, SessionConfiguration cfg)
        {
            return new Hyperstore.Modeling.Commands.HyperstoreTransactionScope(session, cfg.IsolationLevel, cfg.SessionTimeout);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates concurrent dictionary.
        /// </summary>
        /// <typeparam name="TKey">
        ///  Type of the key.
        /// </typeparam>
        /// <typeparam name="TValue">
        ///  Type of the value.
        /// </typeparam>
        /// <returns>
        ///  The new concurrent dictionary.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IConcurrentDictionary<TKey, TValue> CreateConcurrentDictionary<TKey, TValue>()
        {
            return new InternalConcurrentDictionary<TKey, TValue>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Parallel for each.
        /// </summary>
        /// <typeparam name="TSource">
        ///  Type of the source.
        /// </typeparam>
        /// <param name="source">
        ///  Source for the.
        /// </param>
        /// <param name="body">
        ///  The body.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public virtual void Parallel_ForEach<TSource>(System.Collections.Generic.IEnumerable<TSource> source, Action<TSource> body)
        {
            foreach (var s in source)
                body(s);
        }

        internal IConcurrentQueue<TValue> CreateConcurrentQueue<TValue>()
        {
            return new InternalConcurrentQueue<TValue>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the dispatcher.
        /// </summary>
        /// <returns>
        ///  The new dispatcher.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISynchronizationContext CreateDispatcher()
        {
            return new EmptyDispatcher();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates model element factory.
        /// </summary>
        /// <returns>
        ///  The new model element factory.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IModelElementFactory CreateModelElementFactory()
        {
            return new Hyperstore.Modeling.Domain.ModelElementFactory();
        }


        internal void Parallel_ForEach(List<IGrouping<Metadata.Constraints.IConstraintsManager, IModelElement>> constraints, Action<IGrouping<Metadata.Constraints.IConstraintsManager, IModelElement>> action)
        {
            throw new NotImplementedException();
        }
    }
}
