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
     
        IEnumerable<IModelEntity> GetExtensionEntities(ISchemaEntity schemaEntity = null);
        IEnumerable<IModelElement> GetDeletedElements();
     
        IEnumerable<IModelRelationship> GetExtensionRelationships(ISchemaRelationship schemaRelationship = null, IModelElement start = null, IModelElement end = null);

    }
}
