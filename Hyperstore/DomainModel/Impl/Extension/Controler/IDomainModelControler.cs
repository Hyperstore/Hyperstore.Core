using System;
namespace Hyperstore.Modeling.DomainExtension
{
    interface IDomainModelControler<T> : IDisposable
     where T : class, global::Hyperstore.Modeling.IDomainModel
    {
        void ActivateDomain(T domain);
        T GetDomainModel(string name);
        global::System.Collections.Generic.IEnumerable<T> GetDomainModels();
        void OnSessionCreated(global::Hyperstore.Modeling.ISession session);
        void RegisterDomainModel(T domainModel);
        void UnloadDomainExtension(T domainModel);
        System.Collections.Generic.IEnumerable<T> GetAllDomainModelIncludingExtensions();
    }
}
