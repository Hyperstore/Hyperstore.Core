using Hyperstore.Modeling.HyperGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling
{
    internal interface IExtension
    {
    }

    public interface IDomainModelExtension : IDomainModel
    {
        IEnumerable<IModelElement> GetExtensionElements(ISchemaElement schemaElement = null);

        IEnumerable<INodeInfo> GetDeletedElements();
    }
}
