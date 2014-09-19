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
 
#region Imports

using System;
using System.Linq;
using System.Reflection;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling.Domain
{
    internal class ModelElementFactory : IModelElementFactory, IDomainService
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        protected IDomainModel DomainModel { get; private set; }

        void IDomainService.SetDomain(IDomainModel domainModel)
        {
            DebugContract.Requires(domainModel);
            DomainModel = domainModel;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the relationship.
        /// </summary>
        /// <param name="schemaRelationship">
        ///  The schema relationship.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <returns>
        ///  The new relationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelRelationship CreateRelationship(ISchemaRelationship schemaRelationship, IModelElement start, IModelElement end)
        {
            return InvokeModelRelationshipConstructorForType(schemaRelationship, start, end, schemaRelationship.ImplementedType);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the model relationship constructor for type on a different thread, and waits for the
        ///  result.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <param name="start">
        ///  The start.
        /// </param>
        /// <param name="end">
        ///  The end.
        /// </param>
        /// <param name="implementedType">
        ///  Type of the implemented.
        /// </param>
        /// <returns>
        ///  An IModelRelationship.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IModelRelationship InvokeModelRelationshipConstructorForType(ISchemaInfo schemaElement, IModelElement start, IModelElement end, Type implementedType)
        {
            Contract.Requires(schemaElement, "schemaElement");
            Contract.Requires(start, "start");
            Contract.Requires(end, "end");

            var ctor = ReflectionHelper.GetPublicConstructor(implementedType, new[] { typeof(IModelElement), typeof(IModelElement), typeof(ISchemaRelationship) });
            if (ctor != null)
                return (IModelRelationship)ctor.Invoke(new object[] { start, end, schemaElement });

            ctor = ReflectionHelper.GetPublicConstructor(implementedType, new[] { typeof(IModelElement), typeof(IModelElement) });
            if (ctor != null)
                return (IModelRelationship)ctor.Invoke(new object[] { start, end });

            throw new Exception(String.Format(ExceptionMessages.UnableToCreateRelationshipOfTypeRequiredCtorWithStartEndParametersFormat, implementedType));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Creates the element.
        /// </summary>
        /// <param name="schemaEntity">
        ///  The schema entity.
        /// </param>
        /// <returns>
        ///  The new entity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelEntity CreateEntity(ISchemaEntity schemaEntity)
        {
            Contract.Requires(schemaEntity, "schemaEntity");
            return InvokeModelElementConstructorForType(schemaEntity, schemaEntity.ImplementedType);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Executes the model element constructor for type on a different thread, and waits for the
        ///  result.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <param name="implementedType">
        ///  Type of the implemented.
        /// </param>
        /// <returns>
        ///  An IModelEntity.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual IModelEntity InvokeModelElementConstructorForType(ISchemaEntity schemaElement, Type implementedType)
        {
            Contract.Requires(schemaElement, "schemaElement");
            var ctor = ReflectionHelper.GetPublicConstructor(implementedType, new[] { typeof(IDomainModel), typeof(ISchemaEntity) });
            if (ctor != null)
                return (IModelEntity)ctor.Invoke(new object[] { DomainModel, schemaElement });

            ctor = ReflectionHelper.GetPublicConstructor(implementedType, new[] { typeof(IDomainModel) });
            if (ctor != null)
                return (IModelEntity)ctor.Invoke(new object[] { DomainModel });

            throw new Exception(String.Format(ExceptionMessages.UnableToCreateEntityOfTypeCtorWithIDomainModelRequiredFormat, implementedType));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Instanciates the model element.
        /// </summary>
        /// <exception cref="Exception">
        ///  Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="schemaElement">
        ///  The schema element.
        /// </param>
        /// <param name="implementationType">
        ///  Type of the implementation.
        /// </param>
        /// <returns>
        ///  An IModelElement.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IModelElement InstanciateModelElement(ISchemaInfo schemaElement, Type implementationType)
        {
            Contract.Requires(implementationType, "implementationType");
            Contract.Requires(schemaElement, "schemaElement");

            IModelElement mel;
            try
            {
                mel = InstanciateModelElementCore(implementationType);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(ExceptionMessages.UnableToCreateElementOfTypeFormat, implementationType), ex);
            }

            return mel;
        }

        protected virtual IModelElement InstanciateModelElementCore(Type implementationType)
        {
            var ctor = ReflectionHelper.GetConstructor(implementationType).FirstOrDefault();
            if (ctor == null)
                throw new Exception(ExceptionMessages.ElementMustHaveAProtectedParameterlessConstructor);
            return (IModelElement)ctor.Invoke(null);
        }


        protected virtual Type ResolveType(string assemblyQualifiedTypeName)
        {
            DebugContract.RequiresNotEmpty(assemblyQualifiedTypeName);

            return Type.GetType(assemblyQualifiedTypeName);
        }

    }
}