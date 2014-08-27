using Hyperstore.Modeling.HyperGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling
{
    internal interface IScope
    {
    }

    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for domain model extension.
    /// </summary>
    /// <seealso cref="T:IDomainModel"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IDomainScope : IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the extension elements in this collection.
        /// </summary>
        /// <param name="schemaElement">
        ///  (Optional) the schema element.
        /// </param>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the extension elements in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<IModelElement> GetExtensionElements(ISchemaElement schemaElement = null);

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the deleted elements in this collection.
        /// </summary>
        /// <returns>
        ///  An enumerator that allows foreach to be used to process the deleted elements in this
        ///  collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        IEnumerable<GraphNode> GetDeletedElements();
    }
}
