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

    }
}
