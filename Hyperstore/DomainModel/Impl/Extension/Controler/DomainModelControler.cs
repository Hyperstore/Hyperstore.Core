using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hyperstore.Modeling.DomainExtension
{
    internal class DomainModelControler<T> : IDomainModelControler<T> where T : class,IDomainModel 
    {
        // TODO gestion du dechargement d'un domaine
        class Info
        {
            public readonly T DomainModel;
            public bool Enabled;

            public Info(T domain, bool enabled)
            {
                this.DomainModel = domain;
                Enabled = enabled;
            }
        }

        private IImmutableList<T> _domainModelList;
        private readonly Dictionary<string, Info> _domainModels = new Dictionary<string, Info>(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new object();

        public DomainModelControler()
        {
            _domainModelList = ImmutableList.Create<T>();
        }

        void IDisposable.Dispose()
        {
            lock(_sync)
            {
                foreach(var dm in _domainModelList)
                {
                    dm.Dispose();
                }
                _domainModels.Clear();
                _domainModelList = null;
            }
        }

        void IDomainModelControler<T>.ActivateDomain(T domain)
        {
            lock (_sync)
            {
                Info dm;
                if (_domainModels.TryGetValue(domain.Name, out dm))
                {
                    dm.Enabled = true;
                    if( !(domain is ISchema))
                        Interlocked.Exchange(ref _domainModelList,  _domainModelList.Add(domain));
                }
            }
        }

        T IDomainModelControler<T>.GetDomainModel(string name)
        {
            lock (_sync)
            {
                Info dm;
                if (_domainModels.TryGetValue(name, out dm) && dm.Enabled)
                    return dm.DomainModel;
                return null;
            }
        }

        IEnumerable<T> IDomainModelControler<T>.GetDomainModels()
        {
            return _domainModelList;
        }

        void IDomainModelControler<T>.OnSessionCreated(ISession session)
        {
        }

        void IDomainModelControler<T>.RegisterDomainModel(T domain)
        {
            lock (_sync)
            {
                // For schema, the domain is immediatly available
                var isSchema = domain is ISchema;
                _domainModels.Add(domain.Name, new Info(domain, isSchema));
                if (isSchema)
                    Interlocked.Exchange(ref _domainModelList, _domainModelList.Add(domain));
            }
        }

        void IDomainModelControler<T>.UnloadDomainExtension(T domain)
        {
            lock (_sync)
            {
                Info dm;
                if (_domainModels.TryGetValue(domain.Name, out dm))
                {
                    dm.DomainModel.Dispose();
                    _domainModels.Remove(domain.Name);
                    Interlocked.Exchange(ref _domainModelList, _domainModelList.Remove(domain));
                }
            }
        }


        IEnumerable<T> IDomainModelControler<T>.GetAllDomainModelIncludingExtensions()
        {
            return _domainModelList;
        }
    }
}
