using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnnotation
{
    internal class TinyStateMachine
    {
        private readonly ConcurrentBag<(int prev, int next)> _diagram;

        public int? State { get; private set; }

        public TinyStateMachine(List<(int prev, int next)> gram)
        {
            _diagram = new ConcurrentBag<(int prev, int next)>(gram);
            Reset();
        }

        public bool Start(int initState)
        {
            if (State.HasValue)
                return false;
            if (!_diagram.Any(t => t.prev == initState))
                return false;
            State = initState;
            return true;
        }

        public bool Next(int nextState)
        {
            if (!Try(nextState))
                return false;
            State = nextState;
            return true;
        }

        public bool Try(int nextState)
        {
            if (!State.HasValue)
                return false;
            if (!_diagram.Any(t => t.prev == State && t.next == nextState))
                return false;
            return true;
        }

        public void Reset()
        {
            State = null;
        }
    }
}
