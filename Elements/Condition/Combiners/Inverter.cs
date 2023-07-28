namespace BehaviourGraph.Conditions
{
    public class Inverter : IConditional
    {
        public Inverter(IConditional invertCondition)
        {
            _originCondition = invertCondition;
            FriendlyName = "Inverted_" + invertCondition.FriendlyName;
        }

        private IConditional _originCondition;
        public string FriendlyName { get; set; }

        public LeafStatus OnUpdate()
        {
            return _originCondition.OnUpdate() == LeafStatus.Successed ? LeafStatus.Failure : LeafStatus.Successed;
        }
    }
}
