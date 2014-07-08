using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hyperstore.Modeling.Platform.WinPhone
{
    internal class UIDispatcher : Hyperstore.Modeling.ISynchronizationContext
    {
        private readonly System.Windows.Threading.Dispatcher _dispatcher;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Default constructor.
        /// </summary>
        ///-------------------------------------------------------------------------------------------------
        public UIDispatcher()
        {
            _dispatcher = Deployment.Current.Dispatcher;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///  Send this message.
        /// </summary>
        /// <param name="action">
        ///  The action.
        /// </param>
        ///-------------------------------------------------------------------------------------------------
        public Task Send(Action action)
        {
            if (_dispatcher == null)
                throw new Exception("Incorrect UI dispatcher for the context of the current application. Redefines the correct dispatcher in the store.");

            if (_dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                _dispatcher.BeginInvoke(action);    
            }
            
            return Hyperstore.Modeling.Utils.CompletedTask.Default;
        }
    }
}
