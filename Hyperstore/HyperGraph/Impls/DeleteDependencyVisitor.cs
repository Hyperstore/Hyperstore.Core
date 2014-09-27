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
        private List<IDomainCommand> _commands = new List<IDomainCommand>();

        internal List<IDomainCommand> Commands { get { return _commands; } }

        GraphTraversalEvaluatorResult ITraversalVisitor.Visit(GraphPath path)
        {
            var relationship = path.LastTraversedRelationship;
            if (relationship == null)
                return GraphTraversalEvaluatorResult.Continue;

            _commands.Add(new RemoveRelationshipCommand(path.DomainModel, relationship.Id, relationship.SchemaId));
            var schemaRelationship = path.DomainModel.Store.GetSchemaRelationship(relationship.SchemaId);

            if (!schemaRelationship.IsEmbedded || String.Compare(path.EndElement.Id.DomainModelName, path.DomainModel.Name, StringComparison.OrdinalIgnoreCase) != 0)
                return GraphTraversalEvaluatorResult.IncludeAndNextPath;

            if (path.EndElement != null)
            {
                var endSchema = path.DomainModel.Store.GetSchemaElement(relationship.EndSchemaId);
                if (endSchema is ISchemaRelationship)
                    _commands.Add(new RemoveRelationshipCommand(path.DomainModel, path.EndElement.Id, path.EndElement.SchemaId));
                else
                    _commands.Add(new RemoveEntityCommand(path.DomainModel, path.EndElement.Id, path.EndElement.SchemaId));
                return GraphTraversalEvaluatorResult.Continue;
            }
            return GraphTraversalEvaluatorResult.IncludeAndNextPath;
        }
    }
}
