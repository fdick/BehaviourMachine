using BehaviourGraph;
using BehaviourGraph.States;
using BehaviourGraph.Trees;
using UnityEngine;
using UnityEngine.AI;

public class MoveState : State, IUpdatableState, IEndableState
{
    private Transform _body;
    private Vector3 _movePoint;
    private float _speed;
    private Vector3 _originPos;

    public MoveState(Transform body, Vector3 movePoint, float speed)
    {
        _body = body;
        _movePoint = movePoint;
        _speed = speed;
        _originPos = body.position;
    }
    
    
    // public override void InitializeState()
    // {
    //     
    // }
    
    protected override void OnEnterState(Transition transition)
    {
    }
    
    protected override void OnExitState()
    {
    }

    public void UpdateState()
    {
        var translation = (_movePoint - _body.position).normalized * (_speed * Time.deltaTime);
        _body.Translate(translation, Space.World);
    }

    public UpdateStatus EndCondition()
    {
        return (_movePoint - _body.position).magnitude > 1 ? UpdateStatus.Failure : UpdateStatus.Successed;
    }
}
