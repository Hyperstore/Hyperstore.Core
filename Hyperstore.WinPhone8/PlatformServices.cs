using Hyperstore.Modeling.Platform.WinPhone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Modeling.Platform
{
    public class PlatformServicesInstance : Hyperstore.Modeling.Platform.PlatformServices
    {
        public PlatformServicesInstance()
        {
            Current = this;
        }

        public override Modeling.ISynchronizationContext CreateDispatcher()
        {
            return new UIDispatcher();
        }
    }
}
