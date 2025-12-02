
using BehaviourGraph.Visualizer;
using BehaviourGraph.Conditions;
using BehaviourGraph.States;

public class VisualizedWaitCondition : VisualizedCondition
{
    public System.Single waitSeconds;


    public override ICondition GetInstance(Sequence seq)
    {
        return new WaitCondition(waitSeconds);
    }
}