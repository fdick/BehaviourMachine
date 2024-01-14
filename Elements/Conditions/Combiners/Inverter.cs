using System;
using UnityEditor;

namespace BehaviourGraph.Conditions
{
    public class Inverter : ICondition
    {
        public Inverter(ICondition invertCondition)
        {
            ID = new Guid();
            _originCondition = invertCondition;
            FriendlyName = "Inverted_" + invertCondition.FriendlyName;
        }

        private ICondition _originCondition;
        public string FriendlyName { get; set; }
        public Guid ID { get; }

        public UpdateStatus ConditionUpdate()
        {
            return _originCondition.ConditionUpdate() == UpdateStatus.Successed ? UpdateStatus.Failure : UpdateStatus.Successed;
        }
    }
}
