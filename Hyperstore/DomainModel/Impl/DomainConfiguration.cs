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
 
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.HyperGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A domain configuration.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IDomainConfiguration"/>
    ///-------------------------------------------------------------------------------------------------
    public class DomainConfiguration : IDomainConfiguration
    {
        private readonly List<Action<IServicesContainer>> _factories = new List<Action<IServicesContainer>>();
        internal const string PreloadActionKey = "{A8D153D6-EEED-4841-9106-A508CAAB2BCF}";

        internal DomainConfiguration()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Uses the specified factory.
        /// </summary>
        /// <typeparam name="TService">
        ///  Type of the service.
        /// </typeparam>
        /// <param name="factory">
        ///  The factory.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void Using<TService>(Func<IServicesContainer, TService> factory) where TService : class
        {
            Contract.Requires(factory, "factory");

            var f = new Action<IServicesContainer>(r => r.Register<TService>(factory));
            _factories.Add(f);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare services container.
        /// </summary>
        /// <param name="container">
        ///  The default services container.
        /// </param>
        /// <returns>
        ///  An services container.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IServicesContainer IDomainConfiguration.PrepareScopedContainer(IServicesContainer container)
        {
            return PrepareScopedContainer(container);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Prepare services container.
        /// </summary>
        /// <param name="container">
        ///  The default services container.
        /// </param>
        /// <returns>
        ///  An services container.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IServicesContainer PrepareScopedContainer(IServicesContainer container)
        {
            Contract.Requires(container, "container");

            foreach (var action in _factories)
            {
                action(container);
            }

            return container;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Registers the in event bus.
        /// </summary>
        /// <param name="outputProperty">
        ///  The output property.
        /// </param>
        /// <param name="inputProperty">
        ///  (Optional) the input property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SubscribeToEventBus(Messaging.ChannelPolicy outputProperty, Messaging.ChannelPolicy inputProperty = null)
        {
            _factories.Add(new Action<IServicesContainer>(r => r.Register(new Hyperstore.Modeling.RegistrationEventBusSetting { OutputProperty = outputProperty, InputProperty = inputProperty })));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets a property.
        /// </summary>
        /// <param name="key">
        ///  The key.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void SetProperty(string key, object value)
        {
            Contract.RequiresNotEmpty(key, "key");
            _factories.Add(new Action<IServicesContainer>(r => r.RegisterSetting(key, value)));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the preload action.
        /// </summary>
        /// <param name="preloadAction">
        ///  The preload action.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void ExecutePreloadAction(Action<IDomainModel> preloadAction)
        {
            Contract.Requires(preloadAction, "preloadAction");
            var f = new Action<IServicesContainer>(r => r.RegisterSetting(PreloadActionKey, preloadAction));
            _factories.Add(f);
        }
    }
}
