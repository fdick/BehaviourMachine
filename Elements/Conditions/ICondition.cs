using System.Collections;
using UnityEngine;

namespace BehaviourGraph.Conditions
{
    public interface ICondition : IPartOfMachine
    {
        public UpdateStatus ConditionUpdate();
    }
}