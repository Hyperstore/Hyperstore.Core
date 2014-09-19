//	Copyright © 2013 - 2014, Alain Metge. All rights reserved.
//
//		This file is part of Hyperstore (http://www.hyperstore.org)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
 
#region Imports

using System;
using System.Collections.Generic;
using Hyperstore.Modeling.Utils;
using Hyperstore.Modeling.Platform;

#endregion

namespace Hyperstore.Modeling
{
    internal static class CodeMarker
    {
        private static volatile JobScheduler _actions;
        private static readonly object Sync = new object();
        private static Queue<MarkerEntry> _entries;
        private static readonly List<ICodeMarkerListener> Listeners = new List<ICodeMarkerListener>();
        private static bool _activate;
        private static bool _enabled;

        internal static void Mark(string marker)
        {
            if (_enabled == false)
                return;

            EnsureInitialized();

            lock (Sync)
            {
                if (_activate)
                {
                    _entries.Enqueue(new MarkerEntry(marker, DateTime.Now, ThreadHelper.CurrentThreadId));
                    _actions.RequestJob();
                }
            }
        }

        internal static IDisposable MarkBlock(string marker)
        {
            if (_enabled)
            {
                Mark(marker + " Begin");
                return new BlockMarker(marker);
            }

            return Disposables.Empty;
        }

        internal static void InitializeDefault()
        {
            _enabled = true;
            RegisterListener(Platform.PlatformServices.Current.CodeMarkerListener);
            EnableMarkers();
        }

        internal static void Dispose()
        {
            if (_actions != null)
                _actions.Dispose();
        }

        internal static void EnableMarkers()
        {
            if (_enabled == false)
                return;

            lock (Sync)
            {
                _activate = true;
            }
        }

        private static void EnsureInitialized()
        {
            if (_actions == null)
            {
                lock (Sync)
                {
                    if (_actions == null)
                    {
                        _actions = new JobScheduler(Emit, TimeSpan.FromSeconds(2));
                        _entries = new Queue<MarkerEntry>();
                    }
                }
            }
        }

        private static void Emit()
        {
            if (!_activate)
                return;

            MarkerEntry[] entries;
            lock (Sync)
            {
                entries = _entries.ToArray();
                _entries.Clear();
            }

            foreach (var entry in entries)
            {
                foreach (var listener in Listeners)
                {
                    listener.Log(entry.Text, entry.Timestamp, entry.ThreadId);
                }
            }
        }

        internal static void RegisterListener(ICodeMarkerListener listener)
        {
            _enabled = true;
            EnsureInitialized();
            lock (Sync)
            {
                if (!Listeners.Contains(listener))
                    Listeners.Add(listener);
            }
        }

        private class BlockMarker : IDisposable
        {
            private readonly string _text;

            internal BlockMarker(string text)
            {
                _text = text;
            }

            ///-------------------------------------------------------------------------------------------------
            /// <summary>
            ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
            ///  resources.
            /// </summary>
            ///-------------------------------------------------------------------------------------------------
            public void Dispose()
            {
                Mark(_text + " End");
            }
        }
    }
}