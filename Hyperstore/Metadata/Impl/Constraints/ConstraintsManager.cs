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
    internal sealed class ConstraintsManager : IDomainService, IConstraintsManager, IDisposable, IConstraintManagerInternal
    {
        enum ValidationKind
        {
            Check,
            Validate
        }

        private Dictionary<Identity, List<ICheckConstraint>> _checkConstraints;
        private Dictionary<Identity, List<IValidationConstraint>> _validationConstraints;

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
            _checkConstraints = new Dictionary<Identity, List<ICheckConstraint>>();
            _validationConstraints = new Dictionary<Identity, List<IValidationConstraint>>();
        }

        #region Register
        public void AddConstraint(ISchemaProperty property, ICheckValueObjectConstraint constraint)
        {
            var interfaces = ReflectionHelper.GetInterfaces(constraint.GetType());

            var constraintElementType = interfaces.Where(i => ReflectionHelper.IsGenericType(i, typeof(ICheckValueObjectConstraint<>)))
                                            .Select(i => ReflectionHelper.GetGenericArguments(i).First())
                                            .FirstOrDefault();
            if (constraintElementType != null)
            {
                AddConstraint(property.Owner.SchemaInfo, new CheckPropertyConstraintProxy(property, constraintElementType, constraint));
            }

            constraintElementType = interfaces.Where(i => ReflectionHelper.IsGenericType(i, typeof(IValidationConstraint<>)))
                                    .Select(i => ReflectionHelper.GetGenericArguments(i).First())
                                    .FirstOrDefault();
            if (constraintElementType != null)
            {
              // AddConstraint(schema, new ValidationConstraintProxy(schema, CreateValidationHandler(constraintElementType, schema), ""));
            }        
        }

        public void AddConstraint<T>(ISchemaElement schema, ICheckConstraint<T> constraint) where T : IModelElement
        {
            var proxy = new CheckConstraintProxy(typeof(T), constraint);
            AddConstraint(schema, proxy);
        }

        private void AddConstraint(ISchemaElement schema, CheckConstraintProxy proxy)
        {
            List<ICheckConstraint> constraints;
            if (!_checkConstraints.TryGetValue(schema.Id, out constraints))
            {
                constraints = new List<ICheckConstraint>();
                _checkConstraints.Add(schema.Id, constraints);
            }
            constraints.Add(proxy);
        }

        public void AddConstraint<T>(ISchemaElement schema, IValidationConstraint<T> constraint) where T : IModelElement
        {
            var proxy = new ValidationConstraintProxy(typeof(T), constraint, constraint.Category);
            AddConstraint(schema, proxy);
        }

        private void AddConstraint(ISchemaElement schema, ValidationConstraintProxy proxy)
        {
            List<IValidationConstraint> constraints;
            if (!_validationConstraints.TryGetValue(schema.Id, out constraints))
            {
                constraints = new List<IValidationConstraint>();
                _validationConstraints.Add(schema.Id, constraints);
            }
            constraints.Add(proxy);
        }
        #endregion


        public IExecutionResult CheckElements(IEnumerable<IModelElement> elements)
        {
            return CheckOrValidateElements(elements, ValidationKind.Check, "CheckConstraints");
        }

        private IExecutionResult CheckOrValidateElements(IEnumerable<IModelElement> elements, ValidationKind kind, string category)
        {
            DebugContract.Requires(category != null || kind == ValidationKind.Validate);

            elements = elements.ToList();
            if (!elements.Any())
                return ExecutionResult.Empty;

            var categoryTitle = category ?? "ValidationConstraints";
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
                            if (kind == ValidationKind.Check)
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
            List<ICheckConstraint> constraints;
            if (_checkConstraints.TryGetValue(schema.Id, out constraints))
            {
                foreach (ICheckConstraint constraint in constraints)
                {
                    if (Session.Current.CancellationToken.IsCancellationRequested)
                        break;

                    constraint.Check(mel, ctx);
                    var parentSchema = schema.SuperClass;
                    if (parentSchema != null && !parentSchema.IsPrimitive)
                    {
                        CheckElement(ctx, mel, parentSchema);
                    }
                }
            }
        }

        public IExecutionResult ValidateElements(IEnumerable<IModelElement> elements, string category = null)
        {
            return CheckOrValidateElements(elements, ValidationKind.Validate, category);
        }

        private void ValidateElement(ConstraintContext ctx, IModelElement mel, ISchemaElement schema, string category)
        {
            List<IValidationConstraint> constraints;
            if (_validationConstraints.TryGetValue(schema.Id, out constraints))
            {
                foreach (IValidationConstraint constraint in constraints)
                {
                    if (Session.Current.CancellationToken.IsCancellationRequested)
                        break;

                    if (category == null || String.Compare(category, constraint.Category, StringComparison.OrdinalIgnoreCase) == 0)
                        constraint.Validate(mel, ctx);
                    var parentSchema = schema.SuperClass;
                    if (parentSchema != null && !parentSchema.IsPrimitive)
                    {
                        ValidateElement(ctx, mel, parentSchema, category);
                    }
                }
            }
        }

        void IDisposable.Dispose()
        {
            _checkConstraints.Clear();
            _validationConstraints.Clear();
        }
    }
}
