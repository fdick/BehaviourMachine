using BehaviourGraph;
using BehaviourGraph.Conditions;
using UnityEngine;

public class WaitCondition : Condition
{
    private float _waitSeconds;

    public WaitCondition(float waitSeconds)
    {
        _waitSeconds = waitSeconds;
    }
    public override UpdateStatus ConditionUpdate()
    {
        return Time.time > _waitSeconds ? UpdateStatus.Successed :  UpdateStatus.Failure;
    }
}
