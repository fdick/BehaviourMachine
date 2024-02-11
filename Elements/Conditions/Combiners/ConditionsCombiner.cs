using System;
using System.Collections.Generic;
using UnityEditor;

namespace BehaviourGraph.Conditions
{
    public abstract class ConditionsCombiner : ICondition
    {
        protected ConditionsCombiner()
        {
            ID = Guid.NewGuid();
        }

        public string FriendlyName { get; set; }
        public Guid ID { get; }
        public List<ICondition> Conditions { get; protected set; }

        public abstract UpdateStatus ConditionUpdate();
    }
}