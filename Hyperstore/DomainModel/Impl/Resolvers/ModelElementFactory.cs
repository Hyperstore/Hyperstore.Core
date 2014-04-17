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

            var ctor = ReflectionHelper.GetPublicConstructor(implementedType, new[] {typeof (IModelElement), typeof (IModelElement), typeof (ISchemaRelationship)});
            if (ctor != null)
                return (IModelRelationship) ctor.Invoke(new object[] {start, end, schemaElement});

            ctor = ReflectionHelper.GetPublicConstructor(implementedType, new[] {typeof (IModelElement), typeof (IModelElement)});
            if (ctor != null)
                return (IModelRelationship) ctor.Invoke(new object[] {start, end});

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
            var ctor = ReflectionHelper.GetPublicConstructor(implementedType, new[] {typeof (IDomainModel), typeof (ISchemaEntity)});
            if (ctor != null)
                return (IModelEntity) ctor.Invoke(new object[] {DomainModel, schemaElement});

            ctor = ReflectionHelper.GetPublicConstructor(implementedType, new[] {typeof (IDomainModel)});
            if (ctor != null)
                return (IModelEntity) ctor.Invoke(new object[] {DomainModel});

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
        public virtual IModelElement InstanciateModelElement(ISchemaInfo schemaElement, Type implementationType)
        {
            Contract.Requires(implementationType, "implementationType");
            Contract.Requires(schemaElement, "schemaElement");

            IModelElement mel;
            try
            {
                mel = ReflectionHelper.GetUninitializedObject(implementationType);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(ExceptionMessages.UnableToCreateElementOfTypeFormat, implementationType), ex);
            }

            return mel;
        }

#if NETFX_CORE
        protected virtual Type ResolveType(string assemblyQualifiedTypeName)
        {
            DebugContract.RequiresNotEmpty(assemblyQualifiedTypeName);

            return Type.GetType(assemblyQualifiedTypeName);
        }
#else

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Resolve type.
        /// </summary>
        /// <param name="assemblyQualifiedTypeName">
        ///  Name of the assembly qualified type.
        /// </param>
        /// <returns>
        ///  A Type.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        protected virtual Type ResolveType(string assemblyQualifiedTypeName)
        {
            DebugContract.RequiresNotEmpty(assemblyQualifiedTypeName);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            try
            {
                return Type.GetType(assemblyQualifiedTypeName);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var pos = args.Name.IndexOf(',');
            var name = pos > 0
                    ? args.Name.Substring(0, pos)
                            .Trim()
                    : args.Name;
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName()
                            .Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            return asm;
        }
#endif
    }
}