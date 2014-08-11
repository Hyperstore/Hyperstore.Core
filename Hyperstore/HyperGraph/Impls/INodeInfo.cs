using System;
namespace Hyperstore.Modeling.HyperGraph
{
    public interface INodeInfo
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity Id { get; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        Identity SchemaId { get; }
    }
}
