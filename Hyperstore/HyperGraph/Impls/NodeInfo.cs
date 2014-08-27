using System;
namespace Hyperstore.Modeling.HyperGraph
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for node information.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    public abstract class NodeInfo
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="id">
        ///  The identifier.
        /// </param>
        /// <param name="schemaId">
        ///  The identifier of the schema.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        protected NodeInfo(Identity id, Identity schemaId)
        {
            DebugContract.Requires(id);
            DebugContract.Requires(schemaId);
            Id = id;
            SchemaId = schemaId;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the id.
        /// </summary>
        /// <value>
        ///  The identifier.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity Id { get; private set; }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the meta class id.
        /// </summary>
        /// <value>
        ///  The identifier of the schema.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        public Identity SchemaId { get; private set; }
    }
}
