using System;

namespace BehaviourGraph.Debug
{
    public interface IDebugable
    {
        public Action Breakpoint { get; set; }
    }
}
