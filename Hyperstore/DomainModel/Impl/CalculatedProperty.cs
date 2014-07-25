using System;
using System.Collections.Generic;
using System.Text;

namespace Hyperstore.Modeling
{

    class CalculatedProperty : IDisposable
    {
        public Action Handler { get; set; }

        public string PropertyName { get; private set; }

        private readonly HashSet<CalculatedProperty> _targets = new HashSet<CalculatedProperty>();

        public CalculatedProperty(string propertyName)
        {
            DebugContract.RequiresNotEmpty(propertyName);
            PropertyName = propertyName;
        }

        internal T GetResult<T>(Func<T> calculation)
        {
            var result = calculation();
            return result;
        }

        internal void Notify(HashSet<CalculatedProperty> set)
        {
            if (Handler != null)
                Handler();

            if( _targets.Count > 0)
                NotifyTargets(set);
        }

        void IDisposable.Dispose()
        {
            Handler = null;
            _targets.Clear();
        }

        internal void AddTarget(CalculatedProperty calculatedProperty)
        {
            _targets.Add(calculatedProperty);
        }

        internal void NotifyTargets(HashSet<CalculatedProperty> circularGuards = null)
        {
            var set = circularGuards ?? new HashSet<CalculatedProperty>();
            foreach(var target in _targets)
            {
                if (set.Add(target))
                {
                    target.Notify(set);
                }
            }
        }

    }
}
