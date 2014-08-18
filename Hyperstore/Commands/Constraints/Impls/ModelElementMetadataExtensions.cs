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
using Hyperstore.Modeling.Metadata;
using Hyperstore.Modeling.Validations;

#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A model element metadata extensions.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class ModelElementMetadataExtensions
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that constraints the given metaclass. </summary>
        /// <param name="metaclass">    The metaclass to act on. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;IModelElement&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<IModelElement> Constraints(this ISchemaElement metaclass, string propertyName = null)
        {
            Contract.Requires(metaclass, "metaclass");
            return ((ISchema)metaclass.DomainModel).Constraints.On(metaclass, propertyName);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that constraints the given metaclass. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metaclass">    The metaclass to act on. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> Constraints<T>(this ISchemaElement metaclass, string propertyName = null)
        {
            Contract.Requires(metaclass, "metaclass");
            return ((ISchema)metaclass.DomainModel).Constraints.On<T>(metaclass, propertyName);
        }

        #region ISchemaEntity

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this ISchemaEntity metadata, Func<T, bool> expression, string message, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaEntity metadata, Func<T, bool> expression, string message, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this ISchemaEntity metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaEntity metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }
        #endregion

        #region ISchemaEntity<T>

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this SchemaEntity<T> metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this SchemaEntity<T> metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this SchemaEntity<T> metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this SchemaEntity<T> metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }
        #endregion

        #region ISchemaRelationship

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this ISchemaRelationship metadata, Func<T, bool> expression, string message, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaRelationship metadata, Func<T, bool> expression, string message, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this ISchemaRelationship metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaRelationship metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }
        #endregion

        #region ISchemaRelationship<T>

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this SchemaRelationship<T> metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this SchemaRelationship<T> metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this SchemaRelationship<T> metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this SchemaRelationship<T> metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }
        #endregion

        #region ISchemaElement

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this ISchemaElement metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) 
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaElement metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName=null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddConstraint<T>(this ISchemaElement metadata, Func<T, bool> expression, string message, string propertyName = null)
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An IConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static IConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaElement metadata, Func<T, bool> expression, string message, string propertyName = null) 
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return metadata.Schema.Constraints.On<T>(metadata, propertyName)
                    .Check(expression, message)
                    .Implicit();
        }
        #endregion
    }
}