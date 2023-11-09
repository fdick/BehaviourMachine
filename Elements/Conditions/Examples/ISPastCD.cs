using BehaviourGraph;
using BehaviourGraph.Conditions;

namespace BehaviourGraph.Conditions
{
    public class ISPastCD : Condition
    {
        public ISPastCD(ILeaf leaf, float duration)
        {
            _leaf = leaf;
            _duration = duration;
        }

        private ILeaf _leaf;
        private float _duration;

        public override UpdateStatus ConditionUpdate()
        {
            return _leaf.CheckCD(_duration) ? UpdateStatus.Successed : UpdateStatus.Failure;
        }
    }
}