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

using System.Linq;
using Hyperstore.Modeling.Validations;
using System.Collections.Generic;

#endregion

namespace Hyperstore.Modeling.Scopes
{
    /// <summary>
    /// </summary>
    internal class DomainExtensionConstraintsManager : IImplicitDomainModelConstraints, IDomainService
    {
        private readonly IConstraintsManager _extendedDomainConstraints;
        private readonly SchemaConstraintExtensionMode _extensionMode;
        private readonly IServicesContainer _services;
        private IConstraintsManager _domainConstraints;
        private ISchema _schema;

        public ISchema Schema { get { return _schema; } }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="services">
        ///  The services.
        /// </param>
        /// <param name="extendedSchema">
        ///  The extended domain model.
        /// </param>
        /// <param name="mode">
        ///  The mode.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public DomainExtensionConstraintsManager(IServicesContainer services, ISchema extendedSchema, SchemaConstraintExtensionMode mode)
        {
            DebugContract.Requires(services);
            DebugContract.Requires(extendedSchema);

            _extendedDomainConstraints = extendedSchema.Constraints;
            _extensionMode = mode;
            _services = services;
            _schema = extendedSchema;
        }

        void IDomainService.SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel);

            _schema = domainModel as ISchema;
            _domainConstraints = _services.Resolve<IConstraintsManager>();
            var service = _domainConstraints as IDomainService;
            if (service != null)
                service.SetDomain(domainModel);
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
            get {
                var constraints = _domainConstraints as IImplicitDomainModelConstraints;
                if (constraints != null && constraints.HasConstraints)
                    return true;
                constraints = _extendedDomainConstraints as IImplicitDomainModelConstraints;
                return constraints != null && constraints.HasConstraints;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Run implicit validation on an element.
        /// </summary>
        /// <param name="session">
        ///  The session.
        /// </param>
        /// <param name="mel">
        ///  The mel.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void ImplicitValidation(ISession session, IModelElement mel)
        {
            DebugContract.Requires(session);
            DebugContract.Requires(mel);

            var constraints = _domainConstraints as IImplicitDomainModelConstraints;
            if (constraints != null)
                constraints.ImplicitValidation(session, mel);

            var extensionLevel = GetExtensionLevel(mel.SchemaInfo);
            if (extensionLevel == 0)
                return;

            if (extensionLevel == 1 || IsInMode(SchemaConstraintExtensionMode.Inherit))
            {
                constraints = _extendedDomainConstraints as IImplicitDomainModelConstraints;
                if (constraints != null)
                    constraints.ImplicitValidation(session, mel);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates the given elements.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="policyName">
        ///  (Optional) Name of the policy.
        /// </param>
        /// <returns>
        ///  An IExecutionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IExecutionResult Validate(IDomainModel domain, string policyName=null)
        {
            return Validate(domain.GetElements().ToArray(), policyName);
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
            _domainConstraints.AddConstraint(constraint);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Ons the given metadata.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="metadata">
        ///  (Optional) The metadata.
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
            return _domainConstraints.On<T>(metadata, propertyName);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Ons.
        /// </summary>
        /// <param name="metadata">
        ///  (Optional) The metadata.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;IModelElement&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<IModelElement> On(ISchemaElement metadata = null, string propertyName = null)
        {
            return _domainConstraints.On(metadata, propertyName);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Ons.
        /// </summary>
        /// <param name="metadata">
        ///  The metadata.
        /// </param>
        /// <param name="propertyName">
        ///  (Optional) name of the property.
        /// </param>
        /// <returns>
        ///  An IConstraintBuilder&lt;IModelElement&gt;
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IConstraintBuilder<IModelElement> On(string metadata, string propertyName = null)
        {
            Contract.RequiresNotEmpty(metadata, "metadata");
            return _domainConstraints.On(metadata, propertyName);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates the given elements.
        /// </summary>
        /// <param name="elements">
        ///  A variable-length parameters list containing elements.
        /// </param>
        /// <param name="categoryName">
        ///  (Optional) name of the category.
        /// </param>
        /// <returns>
        ///  An IExecutionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IExecutionResult Validate(IEnumerable<IModelElement> elements, string categoryName = null)
        {
            Contract.RequiresNotEmpty(categoryName, "categoryName");
            Contract.Requires(elements, "elements");

            var messages = _domainConstraints.Validate(elements, categoryName);

            var extendedElements = elements.Where(e => GetExtensionLevel(e.SchemaInfo) > 1);

            // Si le domain étendu est en read-only, on n'execute pas ses contraintes
            if (!extendedElements.Any() || IsInMode(SchemaConstraintExtensionMode.Replace))
                return messages;

            return ((IExecutionResultInternal)messages).Merge(_extendedDomainConstraints.Validate(extendedElements, categoryName));
        }

        /// <summary>
        ///     Détecte si la classe courante hérite d'une classe du modèle étendu
        /// </summary>
        /// <param name="metaclass">Metaclass à tester</param>
        /// <returns>Niveau d'extension</returns>
        private int GetExtensionLevel(ISchemaElement metaclass)
        {
            DebugContract.Requires(metaclass);
            var level = 0;
            var sp = metaclass;
            while (sp != null && !sp.IsPrimitive)
            {
                level++;
                if (sp.Schema.InstanceId == _extendedDomainConstraints.Schema.InstanceId)
                    return level;
                sp = sp.SuperClass;
            }
            return 0;
        }

        private bool IsInMode(SchemaConstraintExtensionMode mode)
        {
            return (_extensionMode & mode) == mode;
        }
    }
}