﻿//	Copyright © 2013 - 2014, Alain Metge. All rights reserved.
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
using System.Runtime.CompilerServices;

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
    public abstract class ModelElement : IModelElement, ISerializableModelElement, IDisposable, IEquatable<ModelElement>, IPropertyChangedNotifier, INotifyDataErrorInfo, IDataErrorNotifier
    {
        private static int _globalSequence = 1;
        private IDomainModel _domainModel;
        private Identity _id;
        private ISchemaElement _schema;
        private int _sequence;
        private IHyperstore _store;
        private ModelElementStatus _status;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  The validation messages.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        protected Dictionary<string, List<string>> ValidationMessages;
        private EventHandler<DataErrorsChangedEventArgs> _errorsChangedEventHandler;

        private Lazy<Dictionary<string, CalculatedProperty>> _calculatedProperties;

        void IPropertyChangedNotifier.NotifyCalculatedProperties(string propertyName)
        {
            if (_calculatedProperties.IsValueCreated == false)
                return;

            CalculatedProperty tracker;
            if (!_calculatedProperties.Value.TryGetValue(propertyName, out tracker))
                return;

            if (tracker == null)
                return;

            tracker.NotifyTargets();
        }

#if DEBUG
        internal int Sequence
        {
            get { return _sequence; }
        }
#endif

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Calculated property.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="calculation">
        ///  The calculation.
        /// </param>
        /// <param name="propertyName">
        ///  Name of the property.
        /// </param>
        /// <returns>
        ///  A T.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected T CalculatedProperty<T>(Func<T> calculation, [System.Runtime.CompilerServices.CallerMemberName]string propertyName = null)
        {
            Contract.RequiresNotEmpty(propertyName, "propertyName");
            Contract.Requires(calculation, "calculation");


            if (!(this is INotifyPropertyChanged) || (_schema.Schema.Behavior & DomainBehavior.Observable) != DomainBehavior.Observable)
                return calculation();

            SetCalculatedPropertySource(propertyName);

            var session = EnsuresRunInSession(true);
            try
            {
                var tracker = (session ?? Session.Current) as ISupportsCalculatedPropertiesTracking;
                Debug.Assert(tracker != null);

                CalculatedProperty calculatedProperty = null;
                _calculatedProperties.Value.TryGetValue(propertyName, out calculatedProperty);

                if (calculatedProperty == null)
                {
                    calculatedProperty = new CalculatedProperty(propertyName);
                    _calculatedProperties.Value.Add(propertyName, calculatedProperty);
                }
                if (calculatedProperty.Handler == null)
                {
                    calculatedProperty.Handler = () => ((IPropertyChangedNotifier)this).NotifyPropertyChanged(propertyName);
                }

                using (tracker.PushCalculatedPropertyTracker(calculatedProperty))
                {
                    return calculatedProperty.GetResult(calculation);
                }
            }
            finally
            {
                if (session != null)
                {
                    session.AcceptChanges();
                    session.Dispose();
                }
            }
        }

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
                if (_domainModel == null || _domainModel.IsDisposed)
                    throw new UnloadedDomainException(ExceptionMessages.CantUseElementFromUnloadedDomain);
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

        ModelElementStatus IModelElement.Status
        {
            get { return _status; }
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

        void ISerializableModelElement.OnDeserializing(ISchemaElement schemaElement, IDomainModel domainModel, string key, Identity start, Identity end)
        {
            OnDeserializing(schemaElement, domainModel, key, start, end);
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
        ///  The key.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  .
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected virtual void OnDeserializing(ISchemaElement schemaElement, IDomainModel domainModel, string key, Identity start, Identity end)
        {
            Contract.Requires(schemaElement, "schemaElement");
            Contract.Requires(domainModel, "domainModel");
            Contract.RequiresNotEmpty(key, "key");
            //DebugContract.Requires(Session.Current);

            _id = new Identity(domainModel.Name, key);
            Debug.Assert(domainModel.Name == _id.DomainModelName);

            domainModel.IdGenerator.Set(_id);

            Initialize(schemaElement, domainModel);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Méthode permettant de déplacer l'appel du constructeur dans les classes d'héritage.
        /// </summary>
        /// <exception cref="SessionRequiredException">
        ///  Thrown when a Session Required error condition occurs.
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
        ///-------------------------------------------------------------------------------------------------
        protected virtual void Super(IDomainModel domainModel, ISchemaElement schemaElement, Func<IDomainModel, Identity, ISchemaElement, IDomainCommand> commandFactory, Identity id = null)
        {
            Contract.Requires(domainModel, "domainModel");
            Contract.Requires(commandFactory, "commandFactory");

            if (Session.Current == null)
                throw new SessionRequiredException();

            if (schemaElement == null)
            {
                schemaElement = EnsuresSchemaExists(domainModel, GetType().FullName);
            }

            if (id == null)
                id = domainModel.IdGenerator.NextValue(schemaElement);

            _id = id;

            Initialize(schemaElement, domainModel);

            PersistElement(commandFactory);
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

            metadata = domainModel.Store.GetSchemaElement(name, !(domainModel is ISchema)) ?? new GeneratedSchemaEntity(domainModel as ISchema, name, GetType());
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
            _calculatedProperties = new Lazy<Dictionary<string, CalculatedProperty>>(() => new Dictionary<string, CalculatedProperty>());

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
            if (_store != null && this is INotifyPropertyChanged && (_schema.Schema.Behavior & DomainBehavior.Observable) == DomainBehavior.Observable)
            {
                DomainModel.Events.RegisterForAttributeChangedEvent(this);

                ValidationMessages = new Dictionary<string, List<string>>();
            }
        }

        void IDataErrorNotifier.NotifyDataErrors(ISessionResult result)
        {
            var set = BeginDataErrorNotifications();
            foreach (var msg in result.Messages)
            {
                if (msg.MessageType != MessageType.Info && msg.PropertyName != null && msg.Element != null && msg.Element.Id == _id)
                    AddDataError(set, msg);
            }
            EndDataErrorNotification(set);
        }

        private void AddDataError(HashSet<string> set, DiagnosticMessage msg)
        {
            set.Add(msg.PropertyName);
            List<string> messages;
            if (!ValidationMessages.TryGetValue(msg.PropertyName, out messages))
            {
                messages = new List<string>();
                ValidationMessages.Add(msg.PropertyName, messages);
            }
            messages.Add(msg.Message);

        }

        private HashSet<string> BeginDataErrorNotifications()
        {
            var set = new HashSet<string>(ValidationMessages.Select(m => m.Key));
            ValidationMessages.Clear();
            return set;
        }

        private void EndDataErrorNotification(HashSet<string> set)
        {
            foreach (var property in set)
            {
                NotifyErrorsChanged(property);
            }
        }

        #endregion

        #region Gestion des références

        internal IModelElement GetReference(ref Identity relationshipId, ISchemaRelationship relationshipSchema, bool isOpposite)
        {
            DebugContract.Requires(relationshipSchema, "relationshipSchema");

            var propertyName = isOpposite ? relationshipSchema.EndPropertyName : relationshipSchema.StartPropertyName;
            SetCalculatedPropertySource(propertyName);

            IModelRelationship relationship = null;
            if (relationshipId != null)
                relationship = DomainModel.GetRelationship(relationshipId);

            if (relationship == null)
            {
                var start = isOpposite ? null : this;
                var end = isOpposite ? this : null;
                relationship = DomainModel.GetRelationships(relationshipSchema, start, end).FirstOrDefault();
            }

            if (relationship != null)
            {
                relationshipId = relationship.Id;

                var opposite = isOpposite ? relationship.Start : relationship.End;
                if (opposite != null)
                {
                    var mel = _store.GetElement(opposite.Id);
                    if (mel == null)
                        throw new InvalidElementException(opposite.Id, ExceptionMessages.InvalidReference);

                    return mel;
                }
                return opposite;
            }

            relationshipId = null;
            return null;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Sets calculated property source.
        /// </summary>
        /// <param name="propertyName">
        ///  (Optional) Name of the property.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected void SetCalculatedPropertySource([CallerMemberName]string propertyName = null)
        {
            var domain = this.DomainModel; // Check domain model

            var tracker = Session.Current as ISupportsCalculatedPropertiesTracking;
            if (tracker != null)
            {
                var calculatedProperty = tracker.CurrentTracker;
                if (calculatedProperty != null)
                {
                    CalculatedProperty sourceProperty = null;
                    _calculatedProperties.Value.TryGetValue(propertyName, out sourceProperty);

                    if (sourceProperty == null)
                    {
                        sourceProperty = new CalculatedProperty(propertyName);
                        _calculatedProperties.Value.Add(propertyName, sourceProperty);
                    }


                    sourceProperty.AddTarget(calculatedProperty);
                }
            }
        }

        // Création d'une référence 0..1 à partir de l'élément courant ou vers l'élement courant si opposite vaut true
        internal void SetReference(ref Identity relationshipId, ISchemaRelationship relationshipSchema, IModelElement target, bool opposite)
        {
            DebugContract.Requires(relationshipSchema, "relationshipMetadata");

            using (var session = EnsuresRunInSession())
            {
                var commands = new List<IDomainCommand>();
                IModelRelationship relationship = null;

                // !opposite ? this -> target : target -> this
                IModelElement start = !opposite ? this : null;
                IModelElement end = !opposite ? null : this;

                // Si l'identifiant est fourni c'est que la relation existe surement dèjà
                if (relationshipId != null)
                    relationship = DomainModel.GetRelationship(relationshipId);

                if (relationship == null)
                {
                    relationship = DomainModel.GetRelationships(relationshipSchema, start, end).FirstOrDefault();
                }

                start = !opposite ? this : target;
                end = !opposite ? target : this; 

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

                relationship = null;
                relationshipId = null;

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
            var tmp = PropertyChanged;
            if (tmp != null)
                tmp(this, new PropertyChangedEventArgs(propertyName));
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
        protected ISession EnsuresRunInSession(bool readOnly = false)
        {
            if (Session.Current != null)
                return null;

            if (!(this is INotifyPropertyChanged) || (_schema.Schema.Behavior & DomainBehavior.Observable) != DomainBehavior.Observable)
                throw new SessionRequiredException();

            return _store.BeginSession(new SessionConfiguration { Readonly = readOnly });
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
            // Le finalizer existe pour être certain qu'une instance d'un élément sera bien déréférencée du gestionnaire d'événements,
            // comme cela a été fait, ce n'est plus la peine de l'appeler.
            GC.SuppressFinalize(this);

            try
            {
                var domainModelDisposed = _domainModel != null && _domainModel.IsDisposed;
                if (this is INotifyPropertyChanged)
                {
                    if (!domainModelDisposed && _domainModel.Events != null)
                        _domainModel.Events.UnregisterForAttributeChangedEvent(this);

                }
            }
            catch
            {
                // Domain already disposed
            }

            DisableDataErrorsNotification();

            if (_calculatedProperties != null && _calculatedProperties.IsValueCreated)
            {
                foreach (var p in _calculatedProperties.Value.Values)
                {
                    var disposable = p as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }

            OnDisposing();

            _errorsChangedEventHandler = null;
            _calculatedProperties = null;
            _status = ModelElementStatus.Disposed;
            _domainModel = null;
            _store = null;
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
#if DEBUG
            Debug.WriteLine("Property {0} changed for entity {1}", propertyName, _id);
#endif
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
            var namedElement = this as INamedElement;
            if( namedElement != null )
                return namedElement.Name;

            return String.Format("{0} : {1}", GetType().Name, _id);
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

            var pv = DomainModel.GetPropertyValue(_id, property);
            SetCalculatedPropertySource(property.Name);
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
                throw new PropertyDefinitionException(string.Format(ExceptionMessages.UnknownPropertyFormat, propertyName));
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
        private void NotifyErrorsChanged(string propertyName)
        {
            var tmp = _errorsChangedEventHandler;
            if (tmp != null)
                tmp(this, new DataErrorsChangedEventArgs(propertyName));
        }

        System.Collections.IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            List<String> messages = null;
            if (ValidationMessages != null && propertyName != null)
                ValidationMessages.TryGetValue(propertyName, out messages);
            return messages;
        }

        bool INotifyDataErrorInfo.HasErrors
        {
            get { return ValidationMessages != null && ValidationMessages.Count > 0; }
        }
    }
}