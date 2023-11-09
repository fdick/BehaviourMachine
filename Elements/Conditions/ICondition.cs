using System.Collections;
using UnityEngine;

namespace BehaviourGraph.Conditions
{
    public interface ICondition : IForestObject
    {
        // public string FriendlyName { get; set; }
        public UpdateStatus ConditionUpdate();
    }
}