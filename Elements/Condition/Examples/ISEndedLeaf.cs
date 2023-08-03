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

        public override UpdateStatus OnUpdate()
        {
            return _leaf.OnUpdate() == UpdateStatus.Successed ? UpdateStatus.Successed : UpdateStatus.Failure;
        }
    }
}