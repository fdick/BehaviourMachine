using UnityEngine;
using BehaviourGraph.Visualizer;
using BehaviourGraph;
using BehaviourGraph.States;

public class VisualizedRotateState : VisualizedState
{
    public UnityEngine.Transform body;
    public System.Single rotateSpeed;
    public Vector3 rotateAxis;


    public override IState GetInstance(BehaviourMachine graph)
    {
        return new RotateState(body, rotateSpeed, rotateAxis);
    }
}