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
            var relationship = path.LastTraversedRelationship;
            if (relationship != null)
            {
                _relationshipCommands.Push(new RemoveRelationshipCommand(path.DomainModel, relationship.Id, relationship.SchemaId));
                if (end == null)
                    return GraphTraversalEvaluatorResult.IncludeAndNextPath;
            }
            else
            {
                // Traverse begins by a relationship ?
                relationship = path.EndElement as EdgeInfo;

                if (relationship == null || relationship.EndId == null)
                    return GraphTraversalEvaluatorResult.Continue;

                end = new NodeInfo(relationship.EndId, relationship.EndSchemaId);
            }

            var schemaRelationship = path.DomainModel.Store.GetSchemaRelationship(relationship.SchemaId);
            if (!schemaRelationship.IsEmbedded || String.Compare(end.Id.DomainModelName, path.DomainModel.Name, StringComparison.OrdinalIgnoreCase) != 0)
                return GraphTraversalEvaluatorResult.IncludeAndNextPath;

            var endSchema = path.DomainModel.Store.GetSchemaElement(relationship.EndSchemaId);
            if (endSchema is ISchemaRelationship)
                _relationshipCommands.Push(new RemoveRelationshipCommand(path.DomainModel, end.Id, end.SchemaId));
            else
                _entityCommands.Push(new RemoveEntityCommand(path.DomainModel, end.Id, end.SchemaId));

            return GraphTraversalEvaluatorResult.Continue;
        }
    }
}
