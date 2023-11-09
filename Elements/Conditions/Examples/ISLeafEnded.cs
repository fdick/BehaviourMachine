using BehaviourGraph.Leafs;

namespace BehaviourGraph.Conditions
{
    public class ISLeafEnded : Condition
    {
        public ISLeafEnded(IEndableLeaf leaf)
        {
            _leaf = leaf;
        }
        
        private IEndableLeaf _leaf;

        public override UpdateStatus ConditionUpdate()
        {
            return _leaf.EndCondition() == UpdateStatus.Successed ? UpdateStatus.Successed : UpdateStatus.Failure;
        }
    }
}