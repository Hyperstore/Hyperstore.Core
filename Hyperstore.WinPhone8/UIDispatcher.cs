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
            _dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
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
                throw new Exception("Incorrect UI dispatcher for the context of the current application. Redefines the correct dispatcher in the store.");

            await _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(action));
        }
    }
}
