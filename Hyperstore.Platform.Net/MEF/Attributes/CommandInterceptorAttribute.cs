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
using System.ComponentModel.Composition;
using System;
#endregion

namespace Hyperstore.Modeling.Commands
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Attribute for command interceptor.
    /// </summary>
    /// <seealso cref="T:System.ComponentModel.Composition.ExportAttribute"/>
    ///-------------------------------------------------------------------------------------------------
    [PublicAPI]
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class CommandInterceptorAttribute : ExportAttribute
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="domainModel">
        ///  (Optional) The domain model.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public CommandInterceptorAttribute(string domainModel = null)
            : base(typeof(ICommandInterceptor))
        {
            DomainModel = domainModel;
            Priority = 0;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the domain model.
        /// </summary>
        /// <value>
        ///  The domain model.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public string DomainModel { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets or sets the priority.
        /// </summary>
        /// <value>
        ///  The priority.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public int Priority { get; set; }
    }
}

