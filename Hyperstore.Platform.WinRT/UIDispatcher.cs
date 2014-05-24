using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyperstore.Platform.WinPhone
{
    public class UIDispatcher : Hyperstore.Modeling.ISynchronizationContext
    {
        private Windows.UI.Core.CoreDispatcher _dispatcher;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public UIDispatcher()
        {
            var t = Windows.UI.Core.CoreWindow.GetForCurrentThread();
            if( t != null)
                _dispatcher = t.Dispatcher;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send this message.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public async Task Send(Action action)
        {
            if (_dispatcher == null)
            {
                action();
                await Hyperstore.Modeling.Utils.CompletedTask.Default;
            }
            else
                await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(action));
        }
    }
}
