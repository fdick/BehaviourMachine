using System.Collections.Generic;
using UnityEditor;

namespace BehaviourGraph.Conditions
{
    public abstract class ConditionsCombiner : ICondition
    {
        protected ConditionsCombiner()
        {
            ID = GUID.Generate();
        }

        public string FriendlyName { get; set; }
        public GUID ID { get; }
        public List<ICondition> Conditions { get; protected set; }

        public abstract UpdateStatus ConditionUpdate();
    }
}