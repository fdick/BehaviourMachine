using BehaviourGraph;
using BehaviourGraph.States;
using BehaviourGraph.Trees;
using UnityEngine;

public class DelayState : State, IEndableState
{
    private float _delayTime = 1f;
    private float _startTime;

    public DelayState(float delayTime)
    {
        if (delayTime < 0)
            delayTime = 0;

        _delayTime = delayTime;
    }

    protected override void OnEnterState(Transition transition)
    {
        _startTime = Time.time;
    }

    public UpdateStatus EndCondition()
    {
        if (_delayTime == 0)
            return UpdateStatus.Successed;

        return Time.time >= _startTime + _delayTime  ? UpdateStatus.Successed : UpdateStatus.Failure;
    }
}