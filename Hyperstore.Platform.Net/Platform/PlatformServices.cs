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
    }
}
