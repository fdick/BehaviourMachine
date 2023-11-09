using System;
using BehaviourGraph.Trees;

namespace BehaviourGraph.Leafs
{
    public class ForcedLeaf : Leaf, IEndableLeaf
    {
        public ForcedLeaf(Action onEnter, Action onExit) : base(onEnter, onExit)
        {
        }

        public UpdateStatus EndCondition()
        {
            return UpdateStatus.Successed;
        }
    }
}