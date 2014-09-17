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

using System;
namespace Hyperstore.Modeling.Metadata.Constraints
{
    internal interface IConstraintManagerInternal
    {
        Hyperstore.Modeling.ISessionResult CheckElements(System.Collections.Generic.IEnumerable<Hyperstore.Modeling.IModelElement> elements);

        bool HasImplicitConstraints { get; }

        void AddConstraint(ISchemaProperty property, ICheckValueObjectConstraint constraint);
    }

    public interface IConstraintsManager
    {
        void AddConstraint<T>(Hyperstore.Modeling.ISchemaElement schema, Hyperstore.Modeling.Metadata.Constraints.ICheckConstraint<T> constraint) where T : Hyperstore.Modeling.IModelElement;
        Hyperstore.Modeling.ISessionResult Validate(System.Collections.Generic.IEnumerable<Hyperstore.Modeling.IModelElement> elements, string category = null);

        Hyperstore.Modeling.ISessionResult Validate(IDomainModel domain, string category = null);

    }
}
