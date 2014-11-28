using Hyperstore.Modeling.Commands;
using Hyperstore.Modeling.Traversal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.HyperGraph
{
    internal class DeleteDependencyVisitor : ITraversalVisitor
    {
        private Stack<IDomainCommand> _entityCommands = new Stack<IDomainCommand>();
        private Stack<IDomainCommand> _relationshipCommands = new Stack<IDomainCommand>();

        internal IEnumerable<IDomainCommand> Commands { get { return _relationshipCommands.Concat(_entityCommands); } }

        GraphTraversalEvaluatorResult ITraversalVisitor.Visit(GraphPath path)
        {
            var end = path.EndElement;

            IModelRelationship relationship;
            if (path.LastTraversedRelationship != null)
            {
                relationship = path.DomainModel.GetRelationship( path.LastTraversedRelationship.Id);
                _relationshipCommands.Push(new RemoveRelationshipCommand(path.DomainModel, relationship.Id, relationship.SchemaInfo.Id));
                if (end == null)
                    return GraphTraversalEvaluatorResult.IncludeAndNextPath;
            }
            else
            {
                // Traverse begins by a relationship ?
                relationship = path.DomainModel.GetRelationship( path.EndElement);
                if (relationship == null || relationship.EndId == null)
                    return GraphTraversalEvaluatorResult.Continue;

                end = relationship.EndId;
            }

            var schemaRelationship = relationship.SchemaRelationship;
            if (!schemaRelationship.IsEmbedded || String.Compare(end.DomainModelName, path.DomainModel.Name, StringComparison.OrdinalIgnoreCase) != 0)
                return GraphTraversalEvaluatorResult.IncludeAndNextPath;

            var endSchema = schemaRelationship.End;
            if (endSchema is ISchemaRelationship)
                _relationshipCommands.Push(new RemoveRelationshipCommand(path.DomainModel, end, endSchema.Id));
            else
                _entityCommands.Push(new RemoveEntityCommand(path.DomainModel, end));

            return GraphTraversalEvaluatorResult.Continue;
        }
    }
}
