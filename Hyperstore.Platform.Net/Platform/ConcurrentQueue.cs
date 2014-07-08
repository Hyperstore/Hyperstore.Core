using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Platform.Net
{
    class ConcurrentQueue<TValue> : System.Collections.Concurrent.ConcurrentQueue<TValue>, IConcurrentQueue<TValue>
    {
    }
}
