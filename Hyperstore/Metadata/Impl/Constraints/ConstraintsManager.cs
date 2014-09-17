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

using Hyperstore.Modeling.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Metadata.Constraints
{
    enum ConstraintKind
    {
        Check,
        Validate
    }

    internal class ConstraintsManager : IDomainService, IConstraintsManager, IDisposable, IConstraintManagerInternal
    {
        private Dictionary<Identity, List<ConstraintProxy>> _checkConstraints;

        private IHyperstore Store { get; set; }

        public bool HasImplicitConstraints
        {
            get { return _checkConstraints.Any(); }
        }

        internal ConstraintsManager(IServicesContainer services)
        {
        }

        void IDomainService.SetDomain(IDomainModel domainModel)
        {
            Store = domainModel.Store;
            _checkConstraints = new Dictionary<Identity, List<ConstraintProxy>>();
        }

        #region Register
        public void AddConstraint(ISchemaProperty property, ICheckValueObjectConstraint constraint)
        {
            var owner = property.Owner as ISchemaElement;
            if (owner == null)
                return;

            var interfaces = ReflectionHelper.GetInterfaces(constraint.GetType());

            var constraintElementType = interfaces.Where(i => ReflectionHelper.IsGenericType(i, typeof(IValidationValueObjectConstraint<>)))
                        .Select(i => ReflectionHelper.GetGenericArguments(i).First())
                        .FirstOrDefault();
            if (constraintElementType != null)
            {
                var category = ((IValidationValueObjectConstraint)constraint).Category;
                AddConstraint(owner, new CheckPropertyConstraintProxy(property, constraintElementType, constraint, ConstraintKind.Validate, category));
            }
            else
            {
                constraintElementType = interfaces.Where(i => ReflectionHelper.IsGenericType(i, typeof(ICheckValueObjectConstraint<>)))
                                                .Select(i => ReflectionHelper.GetGenericArguments(i).First())
                                                .FirstOrDefault();
                AddConstraint(owner, new CheckPropertyConstraintProxy(property, constraintElementType, constraint, ConstraintKind.Check, null));
            }
        }

        public void AddConstraint<T>(ISchemaElement schema, ICheckConstraint<T> constraint) where T : IModelElement
        {
            var validation = constraint as IValidationConstraint<T>;
            var category = validation != null ? validation.Category : null;
            var proxy = new ConstraintProxy(typeof(T), constraint, validation != null ? ConstraintKind.Validate : ConstraintKind.Check, category);
            AddConstraint(schema, proxy);
        }

        private void AddConstraint(ISchemaElement schema, ConstraintProxy proxy)
        {
            List<ConstraintProxy> constraints;
            if (!_checkConstraints.TryGetValue(schema.Id, out constraints))
            {
                constraints = new List<ConstraintProxy>();
                _checkConstraints.Add(schema.Id, constraints);
            }
            constraints.Add(proxy);
        }
        #endregion


        public virtual ISessionResult CheckElements(IEnumerable<IModelElement> elements)
        {
            return CheckOrValidateElements(elements, ConstraintKind.Check, null);
        }

        private ISessionResult CheckOrValidateElements(IEnumerable<IModelElement> elements, ConstraintKind kind, string category)
        {
            if (!elements.Any())
                return ExecutionResult.Empty;

            var categoryTitle = category ?? (kind == ConstraintKind.Check ? "CheckConstraints" : "ValidationConstraints");
            using (CodeMarker.MarkBlock("ConstraintsManager." + categoryTitle))
            {
                ISession session = null;
                if (Session.Current == null)
                {
                    session = Store.BeginSession(new SessionConfiguration { Readonly = true });
                }

                var ctx = new ConstraintContext(((ISessionInternal)Session.Current).SessionContext, categoryTitle);
                try
                {
                    foreach (var mel in elements)
                    {
                        ctx.Element = mel;
                        var schema = mel.SchemaInfo;
                        try
                        {
                            if (kind == ConstraintKind.Check)
                            {
                                CheckElement(ctx, mel, schema);
                            }
                            else
                                ValidateElement(ctx, mel, schema, category);
                        }
                        catch (Exception ex)
                        {
                            ((ISessionInternal)session).SessionContext.Log(new DiagnosticMessage(MessageType.Error, ex.Message, categoryTitle, false, mel));
                        }
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

                return ctx.Messages;
            }
        }

        private void CheckElement(ConstraintContext ctx, IModelElement mel, ISchemaElement schema)
        {
            List<ConstraintProxy> constraints;
            if (_checkConstraints.TryGetValue(schema.Id, out constraints))
            {
                foreach (var constraint in constraints.Where(c => c.Kind == ConstraintKind.Check))
                {
                    if (Session.Current.CancellationToken.IsCancellationRequested)
                        break;

                    constraint.ExecuteConstraint(mel, ctx);
                }
            }
            var parentSchema = schema.SuperClass;
            if (parentSchema != null && !parentSchema.IsPrimitive)
            {
                CheckElement(ctx, mel, parentSchema);
            }
        }

        public virtual ISessionResult ValidateElements(IEnumerable<IModelElement> elements, string category = null)
        {
            return CheckOrValidateElements(elements.ToList(), ConstraintKind.Validate, category);
        }

        private void ValidateElement(ConstraintContext ctx, IModelElement mel, ISchemaElement schema, string category)
        {
            List<ConstraintProxy> constraints;
            if (_checkConstraints.TryGetValue(schema.Id, out constraints))
            {
                foreach (var constraint in constraints)
                {
                    if (Session.Current.CancellationToken.IsCancellationRequested)
                        break;

                    if (category == null || String.Compare(category, constraint.Category, StringComparison.OrdinalIgnoreCase) == 0)
                        constraint.ExecuteConstraint(mel, ctx);
                }
            }

            var parentSchema = schema.SuperClass;
            if (parentSchema != null && !parentSchema.IsPrimitive)
            {
                ValidateElement(ctx, mel, parentSchema, category);
            }
        }

        void IDisposable.Dispose()
        {
            _checkConstraints.Clear();
        }
    }
}
