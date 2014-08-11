using Hyperstore.Modeling.HyperGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Domain
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Interface for hyper graph provider.
    /// </summary>
    /// <seealso cref="T:IDomainModel"/>
    ///-------------------------------------------------------------------------------------------------
    public interface IHyperGraphProvider : IDomainModel
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Gets the inner graph.
        /// </summary>
        /// <value>
        ///  The inner graph.
        /// </value>
        ///-------------------------------------------------------------------------------------------------
        IHyperGraph InnerGraph { get; }
    }
}
