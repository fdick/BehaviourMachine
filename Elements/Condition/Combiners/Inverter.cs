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

        public UpdateStatus OnUpdate()
        {
            return _originCondition.OnUpdate() == UpdateStatus.Successed ? UpdateStatus.Failure : UpdateStatus.Successed;
        }
    }
}
