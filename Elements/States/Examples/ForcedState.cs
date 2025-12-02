using System;

namespace BehaviourGraph.States
{
    public class ForcedState : State, IEndableState
    {
        public ForcedState(Action onEnter, Action onExit) : base(onEnter, onExit)
        {
        }

        public UpdateStatus EndCondition()
        {
            return UpdateStatus.Successed;
        }
    }
}