using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.States;

public class VisualizedMoveState : VisualizedState
{
    public UnityEngine.Transform body;
    public UnityEngine.Vector3 movePoint;
    public System.Single speed;

    public override IState GetInstance(BehaviourMachine graph)
    {
        return new MoveState(body, movePoint, speed);
    }
}