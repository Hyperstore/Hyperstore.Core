using System;
using System.Collections.Generic;
namespace Hyperstore.Modeling.Scopes
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for model list.
    /// </summary>
    /// <typeparam name="T">
    ///  Generic type parameter.
    /// </typeparam>
    /// <seealso cref="T:IEnumerable{T}"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IModelList<T> : IEnumerable<T> where T : class, IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the store.
        /// </summary>
        /// <value>
        ///  The store.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IHyperstore Store { get; }
    }

    interface IScopeManager<T> : IModelList<T>, IDisposable where T : class, global::Hyperstore.Modeling.IDomainModel
    {
        void ActivateScope(T scope);
        T GetActiveScope(string name);
        //   global::System.Collections.Generic.IEnumerable<T> GetActiveScopes();
        void OnSessionCreated(global::Hyperstore.Modeling.ISession session);
        void RegisterScope(T scope);
        void UnloadScope(T scope);
        System.Collections.Generic.IEnumerable<T> GetAllScopes();
    }
}
