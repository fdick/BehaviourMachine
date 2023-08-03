using System;

namespace BehaviourGraph.Debug
{
    public interface IDebugBreakpoint
    {
        public Action Breakpoint { get; set; }
    }
}
