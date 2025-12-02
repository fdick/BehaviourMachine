using BehaviourGraph.States;

namespace BehaviourGraph.Conditions
{
    public class ISPastCD : Condition
    {
        public ISPastCD(IState leaf, float duration)
        {
            _leaf = leaf;
            _duration = duration;
        }

        private IState _leaf;
        private float _duration;

        public override UpdateStatus ConditionUpdate()
        {
            return _leaf.CheckCD(_duration) ? UpdateStatus.Successed : UpdateStatus.Failure;
        }
    }
}