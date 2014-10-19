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
using Hyperstore.Modeling.Platform.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Platform
{
    public class PlatformServicesInstance : Hyperstore.Modeling.Platform.PlatformServices
    {
        public PlatformServicesInstance()
        {
            Current = this;
        }

        protected override Modeling.IJsonSerializer CreateObjectSerializer()
        {
            return new Hyperstore.Modeling.Platform.Net.JSonSerializer();
        }

        public override Hyperstore.Modeling.Commands.ITransactionScope CreateTransactionScope(Modeling.Session session, Modeling.SessionConfiguration cfg)
        {
            return new TransactionScopeWrapper(cfg.IsolationLevel, cfg.SessionTimeout);
        }

        public override Modeling.Platform.IConcurrentDictionary<TKey, TValue> CreateConcurrentDictionary<TKey, TValue>()
        {
            return new ConcurrentDictionaryWrapper<TKey, TValue>();
        }

        public override void Parallel_ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            Parallel.ForEach<TSource>(source, body);
        }

        public override Modeling.ISynchronizationContext CreateDispatcher()
        {
            return new UIDispatcher();
        }

        public override Modeling.IModelElementFactory CreateModelElementFactory()
        {
            return new ModelElementFactory();
        }

        //public override ICompositionService CreateCompositionService()
        //{
        //    return new CompositionService();
        //}
    }
}
