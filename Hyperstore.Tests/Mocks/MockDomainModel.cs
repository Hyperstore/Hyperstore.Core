using Hyperstore.Modeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Tests.Mocks
{
    class MockDomainModel :IDomainModel
    {
        private readonly string _name;
        private readonly string _extensionName;
        private static int s_sequence;

        public int Id { get; private set; }

        public MockDomainModel(string name, string extensionName=null)
        {
            _name = name;
            _extensionName = extensionName;
            Id = ++s_sequence;
        }

        Modeling.HyperGraph.TraversalBuilder IDomainModel.Traversal
        {
            get { throw new NotImplementedException(); }
        }

        Modeling.HyperGraph.IIdGenerator IDomainModel.IdGenerator
        {
            get { throw new NotImplementedException(); }
        }

        IEventManager IDomainModel.Events
        {
            get { throw new NotImplementedException(); }
        }

        string IDomainModel.InstanceId
        {
            get { throw new NotImplementedException(); }
        }

        string IDomainModel.Name
        {
            get { return _name; }
        }

        string IDomainModel.ExtensionName
        {
            get { return _extensionName; }
        }

        IHyperstore IDomainModel.Store
        {
            get { throw new NotImplementedException(); }
        }

        Modeling.Commands.ICommandManager IDomainModel.Commands
        {
            get { throw new NotImplementedException(); }
        }

        Modeling.HyperGraph.IIndexManager IDomainModel.Indexes
        {
            get { throw new NotImplementedException(); }
        }

        Modeling.Statistics.DomainStatistics IDomainModel.Statistics
        {
            get { throw new NotImplementedException(); }
        }

        IServicesContainer IDomainModel.Services
        {
            get { throw new NotImplementedException(); }
        }

        TService IDomainModel.ResolveOrRegisterSingleton<TService>(TService service)
        {
            throw new NotImplementedException();
        }

        event EventHandler IDomainModel.DomainLoaded
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event EventHandler IDomainModel.DomainUnloaded
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        IModelElement IDomainModel.GetElement(Identity id, ISchemaElement containerSchema)
        {
            throw new NotImplementedException();
        }

        TElement IDomainModel.GetElement<TElement>(Identity id)
        {
            throw new NotImplementedException();
        }

        IModelEntity IDomainModel.GetEntity(Identity id, ISchemaEntity entitySchema)
        {
            throw new NotImplementedException();
        }

        TElement IDomainModel.GetEntity<TElement>(Identity id)
        {
            throw new NotImplementedException();
        }

        IEnumerable<IModelElement> IDomainModel.GetElements(ISchemaElement containerSchema, int skip)
        {
            throw new NotImplementedException();
        }

        IEnumerable<IModelEntity> IDomainModel.GetEntities(ISchemaEntity entitySchema, int skip)
        {
            throw new NotImplementedException();
        }

        IEnumerable<TElement> IDomainModel.GetEntities<TElement>(int skip)
        {
            throw new NotImplementedException();
        }

        PropertyValue IDomainModel.GetPropertyValue(Identity ownerId, ISchemaElement ownerSchema, ISchemaProperty propertySchema)
        {
            throw new NotImplementedException();
        }

        IModelRelationship IDomainModel.GetRelationship(Identity id, ISchemaRelationship relationshipSchema)
        {
            throw new NotImplementedException();
        }

        TRelationship IDomainModel.GetRelationship<TRelationship>(Identity id)
        {
            throw new NotImplementedException();
        }

        IEnumerable<IModelRelationship> IDomainModel.GetRelationships(ISchemaRelationship relationshipSchema, IModelElement start, IModelElement end, int skip)
        {
            throw new NotImplementedException();
        }

        IEnumerable<TRelationship> IDomainModel.GetRelationships<TRelationship>(IModelElement start, IModelElement end, int skip)
        {
            throw new NotImplementedException();
        }

        TService IDomainModel.Resolve<TService>(bool throwExceptionIfNotExists)
        {
            throw new NotImplementedException();
        }

        void IDomainModel.Configure()
        {
            throw new NotImplementedException();
        }

        Modeling.Events.IEventDispatcher IDomainModel.EventDispatcher
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        Task<IDomainScope> IDomainModel.CreateScopeAsync(string scopeName, IDomainConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        Identity IDomainModel.CreateId(string key, ISchemaElement schemaElement)
        {
            throw new NotImplementedException();
        }

        Identity IDomainModel.CreateId(long key, ISchemaElement schemaElement)
        {
            throw new NotImplementedException();
        }

        bool IDomainModel.IsDisposed
        {
            get { throw new NotImplementedException(); }
        }

        bool IDomainModel.SameAs(IDomainModel domainModel)
        {
            throw new NotImplementedException();
        }

        Task<int> IDomainModel.LoadAsync(Query query, MergeOption option, Modeling.Adapters.IGraphAdapter adapter)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
        }
    }
}
