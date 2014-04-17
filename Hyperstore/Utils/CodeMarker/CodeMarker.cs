// Copyright 2014 Zenasoft.  All rights reserved.
//
// This file is part of Hyperstore.
//
//    Hyperstore is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Hyperstore is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Hyperstore.  If not, see <http://www.gnu.org/licenses/>.
 
#region Imports

using System;
using System.Collections.Generic;
using Hyperstore.Modeling.Utils;

#endregion

namespace Hyperstore.Modeling
{
    internal static class CodeMarker
    {
        private static volatile JobScheduler _actions;
        private static readonly object Sync = new object();
        private static Queue<MarkerEntry> _entries;
        private static readonly List<ICodeMarkerListener> Listeners = new List<ICodeMarkerListener>();
        private static bool _enabled;

        internal static void Mark(string marker)
        {
            EnsureInitialized();

            lock (Sync)
            {
                if (_enabled)
                {
                    _entries.Enqueue(new MarkerEntry(marker, DateTime.Now, ThreadHelper.CurrentThreadId));
                    _actions.RequestJob();
                }
            }
        }

        internal static IDisposable MarkBlock(string marker)
        {
            Mark(marker + " Begin");
            return new BlockMarker(marker);
        }

        internal static void InitializeDefault()
        {
            RegisterListener(new DefaultCodeMarkerListener());
            EnableMarkers();
        }

        internal static void EnableMarkers()
        {
            lock (Sync)
            {
                _enabled = true;
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
            if (!_enabled)
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