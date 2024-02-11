using System.Collections;
using UnityEngine;

namespace BehaviourGraph.Conditions
{
    public interface ICondition : IForestObject
    {
        public UpdateStatus ConditionUpdate();
    }
}