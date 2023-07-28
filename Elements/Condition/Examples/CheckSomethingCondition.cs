using System;

namespace BehaviourGraph.Conditions
{
    public class CheckSomethingCondition : DefaultCondition, IDisposable
    {
        public CheckSomethingCondition(Func<bool> onCheckCondition)
        {
            _onCheckCondition = onCheckCondition;
        }

        private Func<bool> _onCheckCondition;

        public override LeafStatus OnUpdate()
        {
            return _onCheckCondition?.Invoke() == true ? LeafStatus.Successed : LeafStatus.Failure;
        }

        public void Dispose()
        {
            _onCheckCondition = null;
        }
    }
}