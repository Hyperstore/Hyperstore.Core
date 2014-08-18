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
using System.Linq;

#endregion

namespace Hyperstore.Modeling.Validations
{
    internal sealed class ConstraintsManager : IImplicitDomainModelConstraints, IDomainService
    {
        private readonly List<IConstraint> _constraints = new List<IConstraint>();
        private readonly IHyperstore _store;
        private ISchema _schema;

        public ISchema Schema { get { return _schema; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="resolver">
        ///  The resolver.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public ConstraintsManager(IDependencyResolver resolver)
        {
            Contract.Requires(resolver, "resolver");
            _store = resolver.Resolve<IHyperstore>();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance has constraints.
        /// </summary>
        /// <value>
        ///  true if this instance has constraints, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool HasConstraints
        {
            get { return _constraints != null && _constraints.Count > 0; }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds a constraint.
        /// </summary>
        /// <param name="constraint">
        ///  The constraint.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void AddConstraint(IConstraint constraint)
        {
            Contract.Requires(constraint != null, "constraint");
            _constraints.Add(constraint);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Ons the given metadata.
        /// </summary>
        /// <param name="metadata">
        ///  the metadata.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;IModelElement&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<IModelElement> On(string metadata, string propertyName=null)
        {
            Contract.RequiresNotEmpty(metadata, "metadata");
            return On<IModelElement>(_store.GetSchemaElement(metadata), propertyName);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Ons the given metadata.
        /// </summary>
        /// <param name="metadata">
        ///  the metadata.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;IModelElement&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<IModelElement> On(ISchemaElement metadata, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            return On<IModelElement>(metadata, propertyName);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Ons the given metadata.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="metadata">
        ///  (Optional) the metadata.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;T&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<T> On<T>(ISchemaElement metadata = null, string propertyName = null) 
        {
            if (metadata == null)
                metadata = _store.GetSchemaInfo<T>() as ISchemaElement;

            var constraint = new ConstraintProxy<T>(metadata);
            _constraints.Add(constraint);

            var builder = new ConstraintBuilder<T>(constraint, propertyName);
            return builder;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validation d'un élement. La validation consiste à executer toutes les contraintes qui
        ///  s'appliquent sur la metadata de l'élément.
        /// </summary>
        /// <param name="elements">
        ///  The elements.
        /// </param>
        /// <param name="categoryName">
        ///  (Optional) name of the category.
        /// </param>
        /// <returns>
        ///  An IExecutionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IExecutionResult Validate(IEnumerable<IModelElement> elements, string categoryName=null)
        {
            return ValidateCore(categoryName ?? ConstraintsCategory.ExplicitPolicy, elements);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Explicit validation.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="policyName">
        ///  (Optional) name of the policy.
        /// </param>
        /// <returns>
        ///  An IExecutionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IExecutionResult Validate(IDomainModel domain, string policyName = null)
        {
            return ValidateCore(policyName ?? ConstraintsCategory.ExplicitPolicy, domain.GetElements());
        }


        private IExecutionResult ValidateCore(string policyName, IEnumerable<IModelElement> elements)
        {
            if (policyName == ConstraintsCategory.ImplicitPolicy)
                throw new ArgumentException("You can not use Implicit policy directly");

            if (_constraints.Count == 0 || !elements.Any())
                return ExecutionResult.Empty;

            using (CodeMarker.MarkBlock("ConstraintsManager.Validate"))
            {
                ISession session = null;
                IExecutionResult messages;
                if (Session.Current == null)
                {
                    session = _store.BeginSession(new SessionConfiguration { Readonly = true });
                    messages = ((ISessionInternal)session).SessionContext.Result;
                }
                else
                    messages = ((ISessionInternal)Session.Current).SessionContext.Result;

                try
                {
                    foreach (var mel in elements)
                    {
                        ValidateElement(session, mel, policyName);
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

                return messages;
            }
        }


        /// <summary>
        ///     Implicit call lorsque la session se termine
        /// </summary>
        /// <param name="mel">The involved elements.</param>
        /// <param name="session">The session.</param>
        void IImplicitDomainModelConstraints.ImplicitValidation(ISession session, IModelElement mel)
        {
            DebugContract.Requires(mel);
            DebugContract.Requires(session);

            if (_constraints.Count == 0)
                return;

            ValidateElement(session, mel, ConstraintsCategory.ImplicitPolicy);
        }

        private void ValidateElement(ISession session, IModelElement mel, string policyName)
        {
            DebugContract.Requires(session);
            DebugContract.Requires(mel);
            DebugContract.RequiresNotEmpty(policyName);

            foreach (var constraint in _constraints.Where(c => policyName == null || String.Compare(c.Category, policyName, StringComparison.OrdinalIgnoreCase) == 0)
                    .Where(constraint => constraint.ApplyOn(session, mel.SchemaInfo)))
            {
                if (Session.Current.CancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    constraint.Apply(mel, ((ISessionInternal)session).SessionContext);
                }
                catch (Exception ex)
                {
                    ((ISessionInternal)session).SessionContext.Log(new DiagnosticMessage(MessageType.Error, ex.Message, "Validations", false, mel));
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  A constraint proxy.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <seealso cref="T:Hyperstore.Modeling.Validations.IConstraint"/>
        ///-------------------------------------------------------------------------------------------------
        public class ConstraintProxy<T> : IConstraint 
        {
            private readonly ISchemaInfo _metadata;
            private IConstraint<T> _constraint;

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Constructor.
            /// </summary>
            /// <param name="metadata">
            ///  The metadata.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public ConstraintProxy(ISchemaInfo metadata)
            {
                DebugContract.Requires(metadata, "metadata");
                _metadata = metadata;
                Category = ConstraintsCategory.ExplicitPolicy;
            }

            #region IConstraint Members

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Validates the specified value.
            /// </summary>
            /// <param name="value">
            ///  The value.
            /// </param>
            /// <param name="log">
            ///  The log.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public void Apply(IModelElement value, ISessionContext log)
            {
                DebugContract.Requires(value);
                DebugContract.Requires(log);

                if (_constraint == null)
                    return;

                _constraint.Apply((T)value, log);
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Applies the on.
            /// </summary>
            /// <param name="session">
            ///  The session.
            /// </param>
            /// <param name="metadata">
            ///  The metadata.
            /// </param>
            /// <returns>
            ///  true if it succeeds, false if it fails.
            /// </returns>
            ///-------------------------------------------------------------------------------------------------
            public bool ApplyOn(ISession session, ISchemaElement metadata)
            {
                DebugContract.Requires(metadata);
                return metadata != null && metadata.IsA(_metadata);
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Gets or sets the category.
            /// </summary>
            /// <value>
            ///  The category.
            /// </value>
            ///-------------------------------------------------------------------------------------------------
            public string Category { get; set; }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Sets a constraint.
            /// </summary>
            /// <param name="constraint">
            ///  The constraint.
            /// </param>
            ///-------------------------------------------------------------------------------------------------
            public void SetConstraint(IConstraint<T> constraint)
            {
                DebugContract.Requires(constraint, "constraint");

                _constraint = constraint;
            }

            #endregion
        }

        void IDomainService.SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel);
            _schema = domainModel as ISchema;
        }
    }
}