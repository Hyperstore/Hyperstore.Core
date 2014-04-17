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

#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Domain;
using Hyperstore.Modeling.Metadata;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Classe de base pour tous les éléments du modèle.
    /// </summary>
    /// <seealso cref="T:Hyperstore.Modeling.IModelElement"/>
    /// <seealso cref="T:Hyperstore.Modeling.ISerializableModelElement"/>
    /// <seealso cref="T:System.IDisposable"/>
    /// <seealso cref="T:System.IEquatable{Hyperstore.Modeling.ModelElement}"/>
    /// <seealso cref="T:Hyperstore.Modeling.Domain.IPropertyChangedNotifier"/>
    ///-------------------------------------------------------------------------------------------------
    public abstract class ModelElement : IModelElement, ISerializableModelElement, IDisposable, IEquatable<ModelElement>, IPropertyChangedNotifier, INotifyDataErrorInfo
    {
        private static int _globalSequence = 1;
        private IDomainModel _domainModel;
        private Identity _id;
        private ISchemaElement _schema;
        private int _sequence;
        private ModelElementStatus _status = ModelElementStatus.Deserializing;
        private IHyperstore _store;
        private IDisposable _onErrorsSubscription;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The validation messages.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected Dictionary<string, List<string>> ValidationMessages;
        private EventHandler<DataErrorsChangedEventArgs> _errorsChangedEventHandler;

        internal IHyperstore Store
        {
            [DebuggerStepThrough]
            get { return _store; }
        }

        #region IModelElement

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IDomainModel DomainModel
        {
            get
            {
                if (_domainModel == null)
                    throw new Exception(ExceptionMessages.CantUseElementFromUnloadedDomain);
                return _domainModel;
            }
        }

        int IModelElement.Sequence
        {
            get { return _sequence; }
        }

        ISchemaElement IModelElement.SchemaInfo
        {
            [DebuggerStepThrough]
            get { return _schema; }
        }

        ModelElementStatus IModelElement.Status
        {
            get { return _status; }
        }

        Identity IModelElement.Id
        {
            [DebuggerStepThrough]
            get { return _id; }
        }

        IDomainModel IModelElement.DomainModel
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return DomainModel; }
        }

        #endregion

        #region Instanciation

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Méthode permettant aux classes hérités de pouvoir utiliser la méthode Super pour maitriser à
        ///  quel moment se fait l'initialisation.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected ModelElement()
        {
        }

        void ISerializableModelElement.OnDeserializing(ISchemaElement schemaElement, IDomainModel domainModel, string key, Identity start, Identity end, Identity endSchemaId)
        {
            OnDeserializing(schemaElement, domainModel, key, start, end, endSchemaId);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Cette méthode est appelée quand une instance est créée dans le processus de sérialisation.
        ///  Dans ce cas, le constructeur n'est jamais appelé.
        /// </summary>
        /// <param name="schemaElement">
        ///  .
        /// </param>
        /// <param name="domainModel">
        ///  .
        /// </param>
        /// <param name="key">
        ///  .
        /// </param>
        /// <param name="start">
        ///  .
        /// </param>
        /// <param name="end">
        ///  .
        /// </param>
        /// <param name="endSchemaId">
        ///  The end schema identifier.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void OnDeserializing(ISchemaElement schemaElement, IDomainModel domainModel, string key, Identity start, Identity end, Identity endSchemaId)
        {
            Contract.Requires(schemaElement, "schemaElement");
            Contract.Requires(domainModel, "domainModel");
            Contract.RequiresNotEmpty(key, "key");
            //DebugContract.Requires(Session.Current);

            _id = new Identity(domainModel.Name, key);
            Debug.Assert(domainModel.Name == _id.DomainModelName);

            domainModel.IdGenerator.Set(_id);

            Initialize(schemaElement, domainModel);

            // Dans le cas d'une création à partir d'une déserialization (avec un modelelementmetadata), on force le status car
            // si on déserialize l'objet c'est qu'il existe.
            SetStatus(ModelElementStatus.Created);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Méthode permettant de déplacer l'appel du constructeur dans les classes d'héritage.
        /// </summary>
        /// <exception cref="NotInTransactionException">
        ///  Thrown when a Not In Transaction error condition occurs.
        /// </exception>
        /// <param name="domainModel">
        ///  .
        /// </param>
        /// <param name="schemaElement">
        ///  .
        /// </param>
        /// <param name="commandFactory">
        ///  .
        /// </param>
        /// <param name="id">
        ///  (Optional)
        /// </param>
        /// ### <returns>
        ///  .
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Super(IDomainModel domainModel, ISchemaElement schemaElement, Func<IDomainModel, Identity, ISchemaElement, IDomainCommand> commandFactory, Identity id = null)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(commandFactory, "commandFactory");

            if (Session.Current == null)
                throw new NotInTransactionException();

            _status = ModelElementStatus.Creating;

            if (schemaElement == null)
            {
                schemaElement = EnsuresSchemaExists(domainModel, GetType().FullName);
            }

            if (id == null)
                id = domainModel.IdGenerator.NextValue(schemaElement);

            _id = id;

            Initialize(schemaElement, domainModel);

            PersistElement(commandFactory);

            // Comme l'objet peut-être crée au sein d'une transaction, il faut s'assurer de sa validité une fois la transaction terminée.
            // Un objet valide doit avoir un Status diffèrent de 'Disposed'. Ce status est mis à jour en s'abonnant à l'événement de fin 
            // de transaction. Si la transaction s'est terminée par un Rollback, on positionne le status à 'Disposed' pour indiquer que
            // l'objet n'est plus valide. 
            // Le status est vérifié à chaque accés à l'objet et génére une exception si l'objet n'est plus valide.
            if (_store != null && Session.Current != null && _status == ModelElementStatus.Creating)
            {
                // Si on est dans une transaction qui peut être annulée, on s'abonne sur l'évenement de fin
                // de la transaction afin de s'assurer que celle ci se termine correctement sinon on positionne
                // son status à Disposed
                Session.Current.Completing += OnCreationSessionCompleted;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Persist element.
        /// </summary>
        /// <param name="commandFactory">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void PersistElement(Func<IDomainModel, Identity, ISchemaElement, IDomainCommand> commandFactory)
        {
            Contract.Requires(commandFactory, "commandFactory");
            DebugContract.Requires(Session.Current);

            Session.Current.Execute(commandFactory(DomainModel, _id, ((IModelElement)this).SchemaInfo));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Queries if a given ensures schema exists.
        /// </summary>
        /// <param name="domainModel">
        ///  .
        /// </param>
        /// <param name="name">
        ///  The name.
        /// </param>
        /// <returns>
        ///  An ISchemaElement.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual ISchemaElement EnsuresSchemaExists(IDomainModel domainModel, string name)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.RequiresNotEmpty(name, "name");
            DebugContract.Requires(Session.Current);

            var metadata = domainModel.Store.GetSchemaElement(name, false);
            if (metadata != null)
                return metadata;

            Session.Current.AcquireLock(LockType.ExclusiveWait, name);

            metadata = domainModel.Store.GetSchemaElement(name, false) ?? new GeneratedSchemaEntity(domainModel as ISchema, name, GetType());
            return metadata;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Méthode appelée dans le processus de création que celle ci soit faite par new ou par
        ///  sérialisation.
        /// </summary>
        /// <param name="schemaElement">
        ///  The schema container.
        /// </param>
        /// <param name="domainModel">
        ///  The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Initialize(ISchemaElement schemaElement, IDomainModel domainModel)
        {
            Contract.Requires(schemaElement, "schemaElement");
            Contract.Requires(domainModel, "domainModel");
            //DebugContract.Requires(Session.Current);

            _sequence = Interlocked.Increment(ref _globalSequence);

            _store = domainModel.Store;
            _domainModel = domainModel;
            _schema = schemaElement;

            SubscribeOnPropertyChangedEvent();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Disables the data errors notification.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected void DisableDataErrorsNotification()
        {
            ValidationMessages = null;
            if (_onErrorsSubscription != null)
                _onErrorsSubscription.Dispose();
            _onErrorsSubscription = null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Subscribe on property changed event.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void SubscribeOnPropertyChangedEvent()
        {
            // Abonnement aux événements de modification d'une propriété afin de générer l'événement OnPropertyChanged
            // Cet événement ne sera généré que si la classe implémente INotifyPropertyChanged
            if (_store != null && (_status == ModelElementStatus.Creating || _status == ModelElementStatus.Deserializing) && this is INotifyPropertyChanged)
            {
                DomainModel.Events.RegisterForAttributeChangedEvent(this);

                ValidationMessages = new Dictionary<string, List<string>>();
                // DataErrorInfo
                _onErrorsSubscription = DomainModel.Events.OnErrors.Subscribe(result =>
                {
                    var set = new HashSet<string>(ValidationMessages.Select(m => m.Key));
                    ValidationMessages.Clear();

                    foreach (var msg in result.Messages.Where(m => m.ElementId == _id))
                    {
                        if (msg.PropertyName == null)
                            continue;

                        set.Add(msg.PropertyName);
                        List<string> messages;
                        if (!ValidationMessages.TryGetValue(msg.PropertyName, out messages))
                        {
                            messages = new List<string>();
                            ValidationMessages.Add(msg.PropertyName, messages);
                        }
                        messages.Add(msg.Message);
                    }

                    foreach (var property in set)
                    {
                        NotifyErrorsChanged(property);
                    }
                });
            }
        }


        /// <summary>
        ///     Quand la session contenant le processus de création est terminée (commit ou rollback), on positionne le status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreationSessionCompleted(object sender, EventArgs e)
        {
            DebugContract.Requires(sender);
            DebugContract.Requires(e);
            DebugContract.Requires(Session.Current);

            Session.Current.Completing -= OnCreationSessionCompleted;

            // Mise à jour du status et suppression de l'abonnement à l'événement                        
            var session = sender as ISession;
            Debug.Assert(session != null, "session != null");
            SetStatus(!session.IsAborted ? ModelElementStatus.Created : ModelElementStatus.Disposed);
        }

        #endregion

        #region Gestion des références

        internal IModelElement GetReference(ref Identity relationshipId, ISchemaRelationship relationshipSchema, bool wantsOpposite)
        {
            DebugContract.Requires(relationshipSchema, "relationshipSchema");

            ThrowIfDisposed();

            IModelRelationship relationship = null;
            if (relationshipId != null)
                relationship = DomainModel.GetRelationship(relationshipId, relationshipSchema);

            if (relationship == null)
            {
                var start = wantsOpposite ? null : this;
                var end = wantsOpposite ? this : null;
                relationship = DomainModel.GetRelationships(relationshipSchema, start, end).FirstOrDefault();
            }

            if (relationship != null)
            {
                relationshipId = relationship.Id;
                var opposite = wantsOpposite ? relationship.Start : relationship.End;
                if (opposite != null)
                {
                    var metaclass = opposite.SchemaInfo;
                    if (metaclass == null)
                        throw new Exception(ExceptionMessages.InvalidMetaclassForReference);

                    opposite = wantsOpposite ? relationship.Start : relationship.End;
                    if (opposite != null)
                    {
                        var mel = _store.GetElement(opposite.Id, metaclass);
                        if (mel == null)
                            throw new Exception(ExceptionMessages.InvalidReference);

                        return mel;
                    }
                }
            }

            relationshipId = null;
            return null;
        }

        // Création d'une référence 0..1 à partir de l'élément courant ou vers l'élement courant si opposite vaut true
        internal void SetReference(ref Identity relationshipId, ISchemaRelationship relationshipSchema, IModelElement target, bool opposite)
        {
            DebugContract.Requires(relationshipSchema, "relationshipMetadata");

            ThrowIfDisposed();

            using (var session = EnsuresRunInSession())
            {
                var commands = new List<IDomainCommand>();
                IModelRelationship relationship = null;

                // !opposite ? this -> target : target -> this
                var start = !opposite ? this : target;
                var end = !opposite ? target : this;

                // Si l'identifiant est fourni c'est que la relation existe surement dèjà
                if (relationshipId != null)
                    relationship = DomainModel.GetRelationship(relationshipId, relationshipSchema);

                if (relationship == null)
                {
                    relationship = DomainModel.GetRelationships(relationshipSchema, start, end)
                            .FirstOrDefault();
                }

                // Si cette relation existe dèjà mais sur un autre élement, on la supprime
                if (relationship != null)
                {
                    // Si elle existe sur le même élement, c'est bon on la conserve
                    if (end != null && relationship.End.Id == end.Id && start != null && relationship.Start.Id == start.Id)
                    {
                        relationshipId = relationship.Id;
                        return;
                    }

                    // Suppression car elle pointe sur un élement diffèrent
                    commands.Add(new RemoveRelationshipCommand(relationship));
                }

                // Si elle n'a pas été mise à null
                if (end != null && start != null)
                {
                    relationshipId = DomainModel.IdGenerator.NextValue(relationshipSchema);
                    commands.Add(new AddRelationshipCommand(relationshipSchema, start, end, relationshipId));
                }

                Session.Current.Execute(commands.ToArray());

                if (session != null)
                    session.AcceptChanges();
            }
        }

        #endregion

        void IDisposable.Dispose()
        {
            Dispose(false);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="end">
        ///  (Optional)
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<T> GetRelationships<T>(IModelElement end = null) where T : IModelRelationship
        {
            return GetRelationships(_store.GetSchemaRelationship<T>(), end)
                    .Select(rel => (T)rel);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the relationships.
        /// </summary>
        /// <param name="metadata">
        ///  (Optional) the metadata.
        /// </param>
        /// <param name="end">
        ///  (Optional)
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the relationships in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual IEnumerable<IModelRelationship> GetRelationships(ISchemaRelationship metadata = null, IModelElement end = null)
        {
            return DomainModel.GetRelationships(metadata, this, end);
        }

        void IPropertyChangedNotifier.NotifyPropertyChanged(string propertyName)
        {
            DebugContract.RequiresNotEmpty(propertyName);
            if (_status != ModelElementStatus.Disposed)
            {
                var tmp = PropertyChanged;
                if (tmp != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Event queue for all listeners interested in PropertyChanged events.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public event PropertyChangedEventHandler PropertyChanged;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Ensures run in session.
        /// </summary>
        /// <exception cref="SessionRequiredException">
        ///  Thrown when a Session Required error condition occurs.
        /// </exception>
        /// <returns>
        ///  An ISession.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected ISession EnsuresRunInSession()
        {
            if (Session.Current != null)
                return null;

            if (!(this is INotifyPropertyChanged))
                throw new SessionRequiredException();

            return _store.BeginSession();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the creation committed action.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void OnCreationCommitted()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Mise à jour du status de l'objet et suppression des abonnements aux événements si l'objet est
        ///  en 'Disposed'.
        /// </summary>
        /// <param name="status">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected void SetStatus(ModelElementStatus status)
        {
            if (status == ModelElementStatus.Disposed && this is INotifyPropertyChanged)
            {
                _domainModel.Events.UnregisterForAttributeChangedEvent(this);
                DisableDataErrorsNotification();

                // Le finalizer existe pour être certain qu'une instance d'un élément sera bien déréférencée du gestionnaire d'événements,
                // comme cela a été fait, ce n'est plus la peine de l'appeler.
                GC.SuppressFinalize(this);
            }

            // Si il il y a changement de status, on envoi des notifications.
            // Bien tester sur le champ _status sinon boucle infini car Status peut appeler SetStatus
            if (this._status == status)
                return;

            this._status = status;

            if (status == ModelElementStatus.Created)
                OnCreationCommitted();

            if (status == ModelElementStatus.Disposed)
                OnDisposing();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the disposing action.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void OnDisposing()
        {
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Vérification du status de l'objet et génération d'une exception si il n'est plus valide.
        /// </summary>
        /// <exception cref="InvalidElementException">
        ///  Thrown when an Invalid Element error condition occurs.
        /// </exception>
        ///-------------------------------------------------------------------------------------------------
        protected void ThrowIfDisposed()
        {
            if (_status == ModelElementStatus.Disposed)
                throw new InvalidElementException(_id, "Invalid state object. You can not used a disposed model element.");
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Finaliser.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        ~ModelElement()
        {
            Dispose(true);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Releases the unmanaged resources used by the Hyperstore.Modeling.ModelElement and optionally
        ///  releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///  true to release both managed and unmanaged resources; false to release only unmanaged
        ///  resources.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            SetStatus(ModelElementStatus.Disposed);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Notifies the property changed.
        /// </summary>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected void NotifyPropertyChanged(string propertyName)
        {
            ((IPropertyChangedNotifier)this).NotifyPropertyChanged(propertyName);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Removes this instance.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected abstract void Remove();

        void IModelElement.Remove()
        {
            Remove();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///  A string that represents the current object.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return String.Format("{0} : {1}", GetType()
                    .Name, _id);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///  <see cref="T:System.Object" />.
        /// </summary>
        /// <param name="obj">
        ///  The object to compare with the current object.
        /// </param>
        /// <returns>
        ///  true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            var other = obj as ModelElement;
            return ((IEquatable<ModelElement>)this).Equals(other);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Inequality operator.
        /// </summary>
        /// <param name="a">
        ///  The ModelElement to process.
        /// </param>
        /// <param name="b">
        ///  The ModelElement to process.
        /// </param>
        /// <returns>
        ///  The result of the operation.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static bool operator !=(ModelElement a, ModelElement b)
        {
            if (ReferenceEquals(a, b))
                return false;

            if (a == null)
                return b != null;

            return !a.Equals(b);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Equality operator.
        /// </summary>
        /// <param name="a">
        ///  The ModelElement to process.
        /// </param>
        /// <param name="b">
        ///  The ModelElement to process.
        /// </param>
        /// <returns>
        ///  The result of the operation.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public static bool operator ==(ModelElement a, ModelElement b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if ((object)a == null || (object)b == null)
                return false;

            return a.Equals(b);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///  A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        #region IDomainModelTraversal Members

        //IDomainModelTraversalQueryConfiguration IDomainModelTraversal.TraversalConfiguration
        //{
        //    get
        //    {
        //        ThrowIfDisposed();
        //        return new DomainModelTraversalQueryConfiguration(this.Store, new Graph.GraphTraversalQueryConfiguration(Store).StartNode(this));
        //    }
        //}

        #endregion

        #region IEquatable<ModelElement> Members

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Tests if this ModelElement is considered equal to another.
        /// </summary>
        /// <param name="other">
        ///  The model element to compare to this instance.
        /// </param>
        /// <returns>
        ///  true if the objects are considered equal, false if they are not.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        bool IEquatable<ModelElement>.Equals(ModelElement other)
        {
            return other != null && other._id == _id;
        }

        #endregion

        #region Attributs

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property value.
        /// </summary>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public PropertyValue GetPropertyValue(ISchemaProperty property)
        {
            Contract.Requires(property, "property");

            ThrowIfDisposed();
            var pv = DomainModel.GetPropertyValue(_id, ((IModelElement)this).SchemaInfo, property);
            return pv;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the property value.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <returns>
        ///  The property value.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public T GetPropertyValue<T>(string propertyName)
        {
            Contract.RequiresNotEmpty(propertyName, "propertyName");
            ThrowIfDisposed();
            var propertyMetadata = GetOrTryToCreateProperty(propertyName, typeof(T));
            var r = (T)GetPropertyValue(propertyMetadata).Value;
            return r;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets property value.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected void SetPropertyValue<T>(string propertyName, T value)
        {
            Contract.RequiresNotEmpty(propertyName, "protectedName");

            using (var session = EnsuresRunInSession())
            {
                var propertyMetadata = GetOrTryToCreateProperty(propertyName, typeof(T));
                SetPropertyValue(propertyMetadata, value);
                if (session != null)
                    session.AcceptChanges();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or try to create property.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <param name="type">
        ///  The type.
        /// </param>
        /// <returns>
        ///  The or try to create property.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected ISchemaProperty GetOrTryToCreateProperty(string propertyName, Type type)
        {
            DebugContract.RequiresNotEmpty(propertyName, "protectedName");
            DebugContract.Requires(type, "type");

            var propertyMetadata = ((IModelElement)this).SchemaInfo.GetProperty(propertyName);
            if (propertyMetadata == null)
            {
                //Session.Current.AcquireLock(LockType.ExclusiveWait, String.Format("{0}.{1}", Metadata.Id, propertyName));
                //propertyMetadata = Metadata.GetProperty(propertyName);
                //if (propertyMetadata == null)
                //{
                //    var pmetadata = Store.GetMetadata(typeName) as IMetaValue;
                //    if (pmetadata == null)
                //        throw new Exception("Unknow meta value type " + typeName);

                //    propertyMetadata = new MetaProperty(Metadata, propertyName, pmetadata);
                //    Metadata.DefineProperty(propertyMetadata);
                //}
                throw new Exception(string.Format(ExceptionMessages.UnknownPropertyFormat, propertyName));
            }

            return propertyMetadata;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets property value.
        /// </summary>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <param name="value">
        ///  The value.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected void SetPropertyValue(ISchemaProperty property, object value)
        {
            Contract.Requires(property, "property");
            ThrowIfDisposed();

            using (var session = EnsuresRunInSession())
            {
                var cmd = new ChangePropertyValueCommand(this, property, value);
                Session.Current.Execute(cmd);
                if (session != null)
                    session.AcceptChanges();
            }
        }

        #endregion

        event EventHandler<DataErrorsChangedEventArgs> INotifyDataErrorInfo.ErrorsChanged
        {
            add
            {
                if (ValidationMessages == null)
                    return;

                lock (ValidationMessages)
                {
                    _errorsChangedEventHandler += value;
                }
            }
            remove
            {
                if (ValidationMessages == null)
                    return;
                lock (ValidationMessages)
                {
                    _errorsChangedEventHandler -= value;
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Notifies the errors changed.
        /// </summary>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected void NotifyErrorsChanged(string propertyName)
        {
            var tmp = _errorsChangedEventHandler;
            if (tmp != null)
                tmp(this, new DataErrorsChangedEventArgs(propertyName));
        }

        System.Collections.IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            List<String> messages = null;
            if( ValidationMessages != null && propertyName != null)
                ValidationMessages.TryGetValue(propertyName, out messages);
            return messages;
        }

        bool INotifyDataErrorInfo.HasErrors
        {
            get { return ValidationMessages != null && ValidationMessages.Count > 0; }
        }
    }
}