using Hyperstore.Modeling.HyperGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Domain
{
    public interface IHyperGraphProvider : IDomainModel
    {
        IHyperGraph InnerGraph { get; }
    }
}
