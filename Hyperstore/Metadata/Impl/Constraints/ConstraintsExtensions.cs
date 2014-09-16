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
using Hyperstore.Modeling.Metadata.Constraints;
#endregion

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  A model element metadata extensions.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public static class ConstraintExtensions
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that constraints the given metaclass. </summary>
        /// <param name="metaclass">    The metaclass to act on. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;IModelElement&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<IModelElement> Constraints(this ISchemaElement metaclass, string propertyName = null)
        {
            Contract.Requires(metaclass, "metaclass");
            return new ConstraintBuilder<IModelElement>(metaclass, propertyName);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that constraints the given metaclass. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metaclass">    The metaclass to act on. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> Constraints<T>(this ISchemaElement metaclass, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metaclass, "metaclass");
            return new ConstraintBuilder<T>(metaclass, propertyName);
        }

        #region ISchemaEntity

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this ISchemaEntity metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaEntity metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this ISchemaEntity metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaEntity metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
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
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this SchemaEntity<T> metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this SchemaEntity<T> metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this SchemaEntity<T> metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this SchemaEntity<T> metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
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
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this ISchemaRelationship metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaRelationship metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this ISchemaRelationship metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaRelationship metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
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
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this SchemaRelationship<T> metadata, Func<T, bool> expression, string message, string propertyName = null) 
            where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this SchemaRelationship<T> metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this SchemaRelationship<T> metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this SchemaRelationship<T> metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelRelationship
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
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
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this ISchemaElement metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaElement metadata, Func<T, bool> expression, DiagnosticMessage message, string propertyName = null) where T : IModelEntity
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds a constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddConstraint<T>(this ISchemaElement metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelElement
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   An ISchemaElement extension method that adds an implicit constraint. </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        /// <param name="metadata">     The metadata to act on. </param>
        /// <param name="expression">   The expression. </param>
        /// <param name="message">      The message. </param>
        /// <param name="propertyName"> (Optional) name of the property. </param>
        /// <returns>   An ConstraintBuilder&lt;T&gt; </returns>
        ///-------------------------------------------------------------------------------------------------
        public static ConstraintBuilder<T> AddImplicitConstraint<T>(this ISchemaElement metadata, Func<T, bool> expression, string message, string propertyName = null) where T : IModelElement
        {
            Contract.Requires(metadata, "metadata");
            Contract.Requires(expression, "expression");
            Contract.Requires(message, "message");
            return new ConstraintBuilder<T>(metadata, propertyName, expression, message, true);
        }
        #endregion
    }
}