namespace BehaviourGraph.Conditions
{
    public class ISEndedLeaf : DefaultCondition
    {
        public ISEndedLeaf(ILeaf leaf)
        {
            _leaf = leaf;
            FriendlyName = "IsEndedLeaf";
        }
        
        private ILeaf _leaf;

        public override LeafStatus OnUpdate()
        {
            return _leaf.OnUpdate() == LeafStatus.Successed ? LeafStatus.Successed : LeafStatus.Failure;
        }
    }
}