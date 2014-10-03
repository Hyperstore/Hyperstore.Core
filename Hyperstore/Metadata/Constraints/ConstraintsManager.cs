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
 
using Hyperstore.Modeling.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Metadata.Constraints
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Values that represent ConstraintKind.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public enum ConstraintKind
    {
        /// <summary>
        ///  Specifies the check option.
        /// </summary>
        Check,
        /// <summary>
        ///  Specifies the validate option.
        /// </summary>
        Validate
    }

    internal class ConstraintsManager : IDomainService, IConstraintsManager, IDisposable, IConstraintManagerInternal
    {
        private Dictionary<Identity, List<AbstractConstraintProxy>> _checkConstraints;

        private IDomainModel Domain { get; set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets a value indicating whether this instance has implicit constraints.
        /// </summary>
        /// <value>
        ///  true if this instance has implicit constraints, false if not.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public bool HasImplicitConstraints
        {
            get { return _checkConstraints.Any(); }
        }

        internal ConstraintsManager(IServicesContainer services)
        {
        }

        void IDomainService.SetDomain(IDomainModel domainModel)
        {
            Domain = domainModel;
            _checkConstraints = new Dictionary<Identity, List<AbstractConstraintProxy>>();
            Domain.DomainLoaded += OnDomainLoaded;
        }

        private void OnDomainLoaded(object sender, EventArgs e)
        {
            Domain.DomainLoaded -= OnDomainLoaded;
            RegisterFromComposition();
        }
        
        #region Register
        private void RegisterFromComposition()
        {
            var compositionService = Domain.Store.Services.Resolve<ICompositionService>();
            if (compositionService == null)
                return;

            foreach (var constraint in compositionService.GetConstraintsForDomainModel(Domain))
            {
                var ruleType = constraint.Value.GetType();
                var interfaces = ReflectionHelper.GetInterfaces(ruleType);

                var constraintType = interfaces.Where(i => ReflectionHelper.IsGenericType(i, typeof(ICheckConstraint<>)))
                                                .Select(i =>  ReflectionHelper.GetGenericArguments(i).First() )
                                                .FirstOrDefault();
                if (constraintType == null)
                    continue;

                var schemaName = new Identity(Domain.Name, Conventions.ExtractMetaElementName(Domain.Name, constraintType.FullName)).ToString();
                var schemaElement = Domain.Store.GetSchemaElement(schemaName, false);
                if (schemaElement == null)
                {
                    Session.Current.Log(new DiagnosticMessage(MessageType.Warning, String.Format("Composition error - Unknow schema element {0} when trying to add a constraint.", constraintType.FullName), "Composition"));
                    continue;
                }

                var validation = constraint.Value as IValidationConstraint;

                var proxy = validation == null ? new ConstraintProxy(constraintType, constraint.Value, ConstraintKind.Check, null) : new ConstraintProxy(constraintType, constraint.Value, ConstraintKind.Validate, validation.Category);
                AddConstraint(schemaElement, proxy);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds a constraint to 'constraint'.
        /// </summary>
        /// <param name="property">
        ///  The property.
        /// </param>
        /// <param name="constraint">
        ///  The constraint.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void AddConstraint(ISchemaProperty property, IConstraint constraint)
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
                if (constraintElementType != null)
                {
                    AddConstraint(owner, new CheckPropertyConstraintProxy(property, constraintElementType, constraint, ConstraintKind.Check, null));
                }
                else
                {
                    throw new HyperstoreException("Invalid constraint type for property " + property.Name);
                }
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Adds a constraint to 'constraint'.
        /// </summary>
        /// <typeparam name="T">
        ///  Generic type parameter.
        /// </typeparam>
        /// <param name="schema">
        ///  The schema.
        /// </param>
        /// <param name="constraint">
        ///  The constraint.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public void AddConstraint<T>(ISchemaElement schema, ICheckConstraint<T> constraint) where T : IModelElement
        {
            var validation = constraint as IValidationConstraint<T>;
            var category = validation != null ? validation.Category : null;
            var proxy = new ConstraintProxy(typeof(T), constraint, validation != null ? ConstraintKind.Validate : ConstraintKind.Check, category);
            AddConstraint(schema, proxy);
        }

        public void AddConstraint<T>(ICheckConstraint<T> constraint) where T : IModelElement
        {
            AddConstraint<T>(this.Domain.Store.GetSchemaElement<T>(), constraint);
        }

        private void AddConstraint(ISchemaElement schema, AbstractConstraintProxy proxy)
        {
            List<AbstractConstraintProxy> constraints;
            if (!_checkConstraints.TryGetValue(schema.Id, out constraints))
            {
                constraints = new List<AbstractConstraintProxy>();
                _checkConstraints.Add(schema.Id, constraints);
            }
            constraints.Add(proxy);
        }
        #endregion

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Check elements.
        /// </summary>
        /// <param name="elements">
        ///  The elements.
        /// </param>
        /// <returns>
        ///  An ISessionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
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
                    session = Domain.Store.BeginSession(new SessionConfiguration { Readonly = true });
                }

                var ctx = new ConstraintContext(((ISessionInternal)Session.Current).SessionContext, categoryTitle, kind);
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
            List<AbstractConstraintProxy> constraints;
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

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates.
        /// </summary>
        /// <param name="domain">
        ///  The domain.
        /// </param>
        /// <param name="category">
        ///  (Optional) the category.
        /// </param>
        /// <returns>
        ///  An ISessionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public ISessionResult Validate(IDomainModel domain, string category = null)
        {
            Contract.Requires(domain, "domain");
            return Validate(domain.GetElements());
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Validates.
        /// </summary>
        /// <param name="elements">
        ///  The elements.
        /// </param>
        /// <param name="category">
        ///  (Optional) the category.
        /// </param>
        /// <returns>
        ///  An ISessionResult.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public virtual ISessionResult Validate(IEnumerable<IModelElement> elements, string category = null)
        {
            return CheckOrValidateElements(elements.ToList(), ConstraintKind.Validate, category);
        }

        private void ValidateElement(ConstraintContext ctx, IModelElement mel, ISchemaElement schema, string category)
        {
            List<AbstractConstraintProxy> constraints;
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
